module SemVerFake
#r @"./packages/SemVer.FromAssembly/tools/SemVer.FromAssembly.exe"
#r @"packages/FAKE/tools/FakeLib.dll"

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
        | Magnitude.Major -> { version with Major=version.Major+1 }
        | Magnitude.Minor -> { version with Minor=version.Minor+1 }
        | Magnitude.Patch -> { version with Patch=version.Patch+1 }
        | m                -> failwithf "Unknown magnitude %s" (m.ToString())

let getMagnitude original new_=
    let maybeMagnitude = SemVer.getMagnitude original new_
    match maybeMagnitude with 
    | Result.Ok m -> m
    | Result.Error err->
        failwithf "Error: %s" err
