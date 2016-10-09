module SemVerFake
#r @"./packages/SemVer.FromAssembly/tools/SemVer.FromAssembly.exe"
#r @"packages/FAKE/tools/FakeLib.dll"

open Fake.SemVerHelper
open SemVer.FromAssembly
open Fake
open System

let downloadOldVersion package =
    let args=sprintf "install '%s' -ExcludeVersion -o bin/" package
    let timeout = TimeSpan.FromMinutes 5.
    let fileName = findNuget (currentDirectory @@ "tools" @@ "NuGet")
    let result = ProcessHelper.ExecProcessAndReturnMessages (fun info->
                    info.Arguments <- args
                    info.FileName <- fileName
                    info.WorkingDirectory <- ""
                    ) timeout
    if result.ExitCode <> 0 || result.Errors.Count > 0 then failwithf "Error during NuGet download. %s" (toLines result.Errors) 

let bumpVersion magnitude (version:SemVerInfo)=
    match magnitude with
        | Magnitude.Major -> { version with Major=version.Major+1; Minor=0; Patch=0 }
        | Magnitude.Minor -> { version with Minor=version.Minor+1; Patch=0 }
        | Magnitude.Patch -> { version with Patch=version.Patch+1 }
        | m                -> failwithf "Unknown magnitude %s" (m.ToString())

let getMagnitude original new_=
    let maybeMagnitude = SemVer.getMagnitude original new_
    match maybeMagnitude with 
    | Result.Ok m -> m
    | Result.Error err->
        failwithf "Error: %s" err
