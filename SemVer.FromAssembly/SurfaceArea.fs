namespace SemVer

module SurfaceArea =
    open System.Reflection
    open System
    open System.Text.RegularExpressions
    // from https://github.com/Microsoft/visualfsharp/blob/master/src/fsharp/FSharp.Core.Unittests/LibraryTestFx.fs
    // gets string form of public surface area for the assembly
    let get (asm:Assembly) =
    
        // public types only
        let types =
            #if portable7 || portable78 || portable259 || coreclr
            asm.ExportedTypes |> Seq.filter (fun ty -> let ti = ty.GetTypeInfo() in ti.IsPublic || ti.IsNestedPublic) |> Array.ofSeq
            #else
            asm.GetExportedTypes()
            #endif

        // extract canonical string form for every public member of every type
        let getTypeMemberStrings (t : Type) =
            // for System.Runtime-based profiles, need to do lots of manual work
            #if portable7 || portable78 || portable259 || coreclr
            let getMembers (t : Type) =
                let ti = t.GetTypeInfo()
                let cast (info : #MemberInfo) = (t, info :> MemberInfo)
                seq {
                    yield! t.GetRuntimeEvents()     |> Seq.filter (fun m -> m.AddMethod.IsPublic) |> Seq.map cast
                    yield! t.GetRuntimeProperties() |> Seq.filter (fun m -> m.GetMethod.IsPublic) |> Seq.map cast
                    yield! t.GetRuntimeMethods()    |> Seq.filter (fun m -> m.IsPublic) |> Seq.map cast
                    yield! t.GetRuntimeFields()     |> Seq.filter (fun m -> m.IsPublic) |> Seq.map cast
                    yield! ti.DeclaredConstructors  |> Seq.filter (fun m -> m.IsPublic) |> Seq.map cast
                    yield! ti.DeclaredNestedTypes   |> Seq.filter (fun ty -> ty.IsNestedPublic) |> Seq.map cast
                } |> Array.ofSeq

            getMembers t
            |> Array.map (fun (ty, m) -> sprintf "%s: %s" (ty.ToString()) (m.ToString()))
            #else
            t.GetMembers()
            |> Array.map (fun v -> sprintf "%s: %s" (v.ReflectedType.ToString()) (v.ToString()))
            #endif
            
        let actual =
            types 
            |> Array.collect getTypeMemberStrings
            |> Array.sort
            |> String.concat "\r\n"

        actual
  


        (*  
    // verify public surface area matches expected
    let verify expected platform fileName =  
        let workDir = TestContext.CurrentContext.WorkDirectory
        let logFile = sprintf "%s\\CoreUnit_%s_Xml.xml" workDir platform
        let normalize (s:string) =
            Regex.Replace(s, "(\\r\\n|\\n)+", "\r\n").Trim([|'\r';'\n'|])
        let asm, actualNotNormalized = getActual ()
        let actual = actualNotNormalized |> normalize
        let expected = expected |> normalize
        
        Assert.AreEqual(expected, actual, sprintf "\r\nAssembly: %A\r\n\r\n%s\r\n\r\n Expected and actual surface area don't match. To see the delta, run\r\nwindiff %s %s" asm actual fileName logFile)
        *)