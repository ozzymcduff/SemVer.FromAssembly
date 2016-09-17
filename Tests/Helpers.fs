module Helpers

open SemVer.FromAssembly

let transform (input:((string*(string*string list)list))list)=
    let mapToSurface t=
        { Members = Set.ofList t}
    let mapToAdts ts=
            ts 
            |> List.map ( fun (name,t)->(name, mapToSurface t ))
            |> Map.ofList

    let namespaces =
        input 
        |> List.map (fun (ns,ts)-> (ns,{ Adts = mapToAdts ts })) 
        |> Map.ofSeq
    {Namespaces=namespaces}
