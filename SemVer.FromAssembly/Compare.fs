namespace SemVer.FromAssembly
open System

// === types repressenting package surface area

type SurfaceOfType ={ Members:Set<string> }

type Namespace=
    {
        adts: Map<string,SurfaceOfType>
    }
type Package=
    {
        Namespaces: Map<string, Namespace>
    }


// === types repressenting package surface area comparison

type AddedAndRemoved<'T when 'T : comparison>=
    {
        Added: Set<'T>
        Removed: Set<'T>
    }

type NamespaceChanges=
    {
        Adt: AddedAndRemoved<string>
        AdtChanges: Map<string,AddedAndRemoved<string>>
    }

type PackageChanges=
    { 
        Namespaces : AddedAndRemoved<string>
        NamespacesChanged: Map<string,NamespaceChanges>
    }
// 
module Map=
    let keys map = map |> Map.toSeq |> Seq.map fst
    let values map = map |> Map.toSeq |> Seq.map snd

module Compare=

    let compareTypes (oldT:SurfaceOfType) (newT:SurfaceOfType) : AddedAndRemoved<string>=
        let newM = (newT.Members) 
        let oldM = (oldT.Members)
        {
           Added = Set.difference newM oldM
           Removed = Set.difference oldM newM
        }
    let typeComparisonIsEmpty (c:AddedAndRemoved<string>)=
        (Set.isEmpty c.Added) && (Set.isEmpty c.Removed) 
            

    let compareNamespaces (oldNs:Namespace) (newNs:Namespace) : NamespaceChanges=
        let newTs = (newNs.adts) |> Map.keys |> set
        let oldTs = (oldNs.adts) |> Map.keys |> set

        let maybeChanged = Set.intersect newTs oldTs
        let compareTypesWithName t=
            let newT = newNs.adts.Item t
            let oldT = oldNs.adts.Item t
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
    let addedAndRemovedMagnitude (ar:AddedAndRemoved<string>)=                             
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
