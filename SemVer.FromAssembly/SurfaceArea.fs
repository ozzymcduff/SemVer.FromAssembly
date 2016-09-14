namespace SemVer.FromAssembly

module SurfaceArea =
    open System.Reflection
    open System
    open System.Text.RegularExpressions
    // from https://github.com/Microsoft/visualfsharp/blob/master/src/fsharp/FSharp.Core.Unittests/LibraryTestFx.fs
    // gets public surface area for the assembly
    let get (asm:Assembly)=
    
        // public types only
        let types =
            asm.GetExportedTypes()

        //string list*Map<string,Type list>
        let getTypeMembers (t : Type) =
            t.GetMembers()
            |> Array.map (fun v -> v.ToString())
                             
        let actual =
            types 
            |> Array.map (fun t-> (t.Namespace, (t.Name, getTypeMembers t)))
            |> Array.groupBy (fun (ns,_)->ns)
            |> Array.map (fun (ns,ns_ts)-> (ns,ns_ts |> Array.map snd)) 
        actual
