namespace SemVer.FromAssembly
open System
open Chiron
open Operators
// === types repressenting package surface area

type SurfaceOfType ={ Members:Set<string> }
    with
      static member ToJson (x:SurfaceOfType) =
           Json.write "members" x.Members
      static member FromJson (_:SurfaceOfType) =
            fun n ->
              { Members = n}
        <!> Json.read "members"

type Namespace=
    {
        Adts: Map<string,SurfaceOfType>
    }
    with
      static member ToJson (x:Namespace) =
           Json.write "adts" x.Adts
      static member FromJson (_:Namespace) =
            fun n ->
              { Adts = n}
        <!> Json.read "adts"

type Package=
    {
        Namespaces: Map<string, Namespace>
    }
    with
      static member ToJson (x:Package) =
           Json.write "namespaces" x.Namespaces
      static member FromJson (_:Package) =
            fun n ->
              { Namespaces = n}
        <!> Json.read "namespaces"


// === types repressenting package surface area comparison

type AddedAndRemoved=
    {
        Added: Set<string>
        Removed: Set<string>
    }
    with
      static member ToJson (x:AddedAndRemoved) =
               Json.write "added"  x.Added
            *> Json.write "removed" x.Removed

type NamespaceChanges=
    {
        Adt: AddedAndRemoved
        AdtChanges: Map<string,AddedAndRemoved>
    }
    with
      static member ToJson (x:NamespaceChanges) =
               Json.write "adt"  x.Adt
            *> Json.write "adtChanges" x.AdtChanges

type PackageChanges=
    { 
        Namespaces : AddedAndRemoved
        NamespacesChanged: Map<string,NamespaceChanges>
    }
    with
      static member ToJson (x:PackageChanges) =
               Json.write "namespaces"  x.Namespaces
            *> Json.write "namespacesChanged" x.NamespacesChanged

module Map=
    let keys map = map |> Map.toSeq |> Seq.map fst
    let values map = map |> Map.toSeq |> Seq.map snd

module Compare=

    let compareTypes (oldT:SurfaceOfType) (newT:SurfaceOfType) : AddedAndRemoved=
        let newM = (newT.Members) 
        let oldM = (oldT.Members)
        {
           Added = Set.difference newM oldM
           Removed = Set.difference oldM newM
        }
    let typeComparisonIsEmpty (c:AddedAndRemoved)=
        (Set.isEmpty c.Added) && (Set.isEmpty c.Removed) 
            

    let compareNamespaces (oldNs:Namespace) (newNs:Namespace) : NamespaceChanges=
        let newTs = (newNs.Adts) |> Map.keys |> set
        let oldTs = (oldNs.Adts) |> Map.keys |> set

        let maybeChanged = Set.intersect newTs oldTs
        let compareTypesWithName t=
            let newT = newNs.Adts.Item t
            let oldT = oldNs.Adts.Item t
            compareTypes oldT newT

        let changed = maybeChanged 
                    |> Seq.map (fun t->t, compareTypesWithName t ) 
                    |> Seq.filter (fun (_,c)-> not (typeComparisonIsEmpty c))
                    |> Map.ofSeq
        {
            Adt={ 
                Added=Set.difference newTs oldTs
                Removed=Set.difference oldTs newTs
            }
            AdtChanges=changed

        }

    let namespaceComparisonIsEmpty (n:NamespaceChanges)=
        (Set.isEmpty n.Adt.Added) && (Set.isEmpty n.Adt.Removed) && (Map.isEmpty n.AdtChanges)


    let comparePackages (oldP:Package) (newP:Package) : PackageChanges= 
        let newNss = (newP.Namespaces) |> Map.keys |> set
        let oldNss = (oldP.Namespaces) |> Map.keys |> set
        let maybeChanged = Set.intersect newNss oldNss
        let compareNsWithName ns=
            let oldN=oldP.Namespaces.Item ns
            let newN=newP.Namespaces.Item ns 
            compareNamespaces oldN newN
        let changed = maybeChanged 
                    |> Seq.map (fun ns-> ns, (compareNsWithName ns ) ) 
                    |> Seq.filter (fun (_,cn) -> not (namespaceComparisonIsEmpty cn) )
                    |> Map.ofSeq

        {
           Namespaces = 
                { 
                    Added = Set.difference newNss oldNss
                    Removed = Set.difference oldNss newNss
                }
           NamespacesChanged = changed
        }
    let addedAndRemovedMagnitude (ar:AddedAndRemoved)=                             
      let added =
          if Seq.isEmpty ar.Added then
            Magnitude.Patch
          else
            Magnitude.Minor
      let removed =
          if Seq.isEmpty ar.Removed then
            Magnitude.Patch
          else
            Magnitude.Major
      [added; removed] |> List.max

    let namespaceChangeMagnitude (mc:NamespaceChanges)=                             
      let addedAndRemoved = addedAndRemovedMagnitude mc.Adt
      let changes =
          mc.AdtChanges
          |> Map.values 
          |> Seq.map addedAndRemovedMagnitude
          |> List.ofSeq
      addedAndRemoved:: changes |> List.max
                               
    let packageChangeMagnitude (pc:PackageChanges)=
      let addedAndRemoved = addedAndRemovedMagnitude pc.Namespaces
      let changes =
          pc.NamespacesChanged
          |> Map.values  
          |> Seq.map namespaceChangeMagnitude
          |> List.ofSeq
      addedAndRemoved:: changes |> List.max
