// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open Fake.VersionHelper
open System
open System.IO

// File system information 
let solutionFile  = "SemVer.FromAssembly.sln"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "**/bin/Release/*Tests.dll"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|) (projFileName:string) = 
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)


// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the 
// src folder to support multiple project outputs
Target "CopyBinaries" (fun _ ->
    !! "src/**/*.??proj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) @@ "bin/Release", "bin" @@ (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "clean" (fun _ ->
    CleanDirs ["bin"; "temp"; 
            "Tests/bin/Debug";
            "Tests/bin/Debug"
            ] 
)

Target "build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "test" (fun _ ->
    !! testAssemblies
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)


Target "pack" (fun _ ->
    Paket.Pack(fun p -> 
        { p with
            OutputPath = "bin"})
)

Target "push" (fun _ ->
    Paket.Push(fun p -> 
        { p with
            WorkingDir = "bin" })
)

#r @"./packages/SemVer.FromAssembly/tools/SemVer.FromAssembly.exe"
open SemVer.FromAssembly
Target "bump" (fun _ ->
    let compiled = "./SemVer.FromAssembly/bin/Release/SemVer.FromAssembly.exe"
    let version = GetAssemblyVersionString compiled
    // Paket install the latest version OR use command line nuget:
    (*
    ProcessHelper.shellExec { 
        CommandLine=sprintf "install SemVer.FromAssembly -Version %s -ExcludeVersion -o bin/" version
        Program="nuget" // using nuget gem ...
        WorkingDirectory=""
        Args=[]
    }*)
    SemVer.getMagnitude "./packages/SemVer.FromAssembly/tools/SemVer.FromAssembly.exe" compiled
    |> printf "%A"
    () 
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "all" DoNothing
"clean"
  ==> "build"
  ==> "pack"

"clean"
  ==> "build"
  ==> "CopyBinaries"
  ==> "test"
  ==> "all"

"pack"
  ==> "push"

RunTargetOrDefault "test"
