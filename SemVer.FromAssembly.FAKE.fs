module SemVerFake

open Fake.SemVerHelper
open SemVer.FromAssembly
open Fake
open System

let downloadOldVersion package version=
    let args=sprintf "install '%s' -Version %s -ExcludeVersion -o bin/" package (version.ToString())
    let timeout = new TimeSpan(0,5,0)
    let result = ProcessHelper.ExecProcessAndReturnMessages (fun info->
                    info.Arguments <- args
                    info.FileName <- "./.nuget/nuget.exe" 
                    info.WorkingDirectory <- ""
                    ) timeout
    if result.ExitCode <> 0 || result.Errors.Count > 0 then failwithf "Error during NuGet download. %s" (toLines result.Errors) 

let bumpVersion magnitude (version:SemVerInfo)=
    match magnitude with
        | Magnitude.Major m-> { version with Major=version.Major+1 }
        | Magnitude.Minor m-> { version with Minor=version.Minor+1 }
        | Magnitude.Patch m-> { version with Patch=version.Patch+1 }
