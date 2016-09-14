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
        AdtAdded: Set<string>
        AdtChanges: Map<string,AddedAndRemoved<string>>
        AdtRemoved: Set<string>
    }

type PackageChanges=
    { 
        NamespacesAdded : Set<string>
        NamespacesChanged: Map<string,NamespaceChanges>
        NamespacesRemoved: Set<string>
    }
// 
module Map=
    let keys map = map |> Map.toSeq |> Seq.map fst

module Compare=

    let compareTypes (oldT:SurfaceOfType) (newT:SurfaceOfType) : AddedAndRemoved<string>
                        =
                        let newM = (newT.Members) 
                        let oldM = (oldT.Members)
                        {
                           Added = Set.difference newM oldM
                           Removed = Set.difference oldM newM
                        }
    let typeComparisonIsEmpty (c:AddedAndRemoved<string>)
                        = (Set.isEmpty c.Added) && (Set.isEmpty c.Removed) 
            

    let compareNamespaces (oldNs:Namespace) (newNs:Namespace) : NamespaceChanges
                        =
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
                            AdtAdded=Set.difference newTs oldTs
                            AdtChanges=changed
                            AdtRemoved=Set.difference oldTs newTs
                        }

    let namespaceComparisonIsEmpty (n:NamespaceChanges)
                        = (Set.isEmpty n.AdtAdded) && (Set.isEmpty n.AdtRemoved) && (Map.isEmpty n.AdtChanges)


    let comparePackages (oldP:Package) (newP:Package) : PackageChanges
                        = 
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
                             NamespacesAdded= Set.difference newNss oldNss
                             NamespacesRemoved=Set.difference oldNss newNss
                             NamespacesChanged = changed
                          }
                             
                         
    