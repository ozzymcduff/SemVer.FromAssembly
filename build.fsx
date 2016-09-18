// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO

// File system information 
let solutionFile  = "SemVer.FromAssembly.sln"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "bin/**/*Tests.dll"

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
    !! "**/*.??proj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) @@ "bin/Release", "bin" @@ (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "clean" (fun _ ->
    CleanDirs ["bin"; "temp"; "NuGet";
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


Target "push" (fun _ ->
    Paket.Push(fun p -> 
        { p with
            WorkingDir = "NuGet" })
)
let release = LoadReleaseNotes "RELEASE_NOTES.md"

Target "pack" (fun _ ->
    let authors = ["Oskar Gewalli"]
    let package = "SemVer.FromAssembly"
    let description = "Guess SemVer based on public surface changes"
    let summary = "Helper tool to guess SemVer version based on public surface area differences"
    let nugetDir = "./NuGet"
    let buildDir = "./SemVer.FromAssembly/bin/Release/"
    let files = [
                  (@"tools/*.dll", Some "tools", None)
                  (@"tools/*.exe", Some "tools", None)
                ]
    let nugetToolsDir = nugetDir @@ "tools"
    CleanDir nugetToolsDir
    !! (buildDir @@ "**/*.*") |> Copy nugetToolsDir
    let setParams p =
        {p with
            Authors = authors
            Project = package
            Description = description
            Version = release.NugetVersion
            OutputPath = nugetDir
            ReleaseNotes = release.Notes |> toLines
            Publish = false
            Summary = summary
            Files = files
        }
    NuGet setParams "./SemVer.FromAssembly/SemVer.FromAssembly.nuspec"
)

#load "SemVer.FromAssembly.FAKE.fsx"
Target "bump" (fun _ ->
    let compiled = "./SemVer.FromAssembly/bin/Release/SemVer.FromAssembly.exe"
    let version = release.NugetVersion
    SemVerFake.downloadOldVersion "SemVer.FromAssembly" version
    let magnitude = SemVerFake.getMagnitude "./bin/SemVer.FromAssembly/tools/SemVer.FromAssembly.exe" compiled
    let version = SemVerFake.bumpVersion magnitude (SemVerHelper.parse version)
    let orig= File.ReadAllText "RELEASE_NOTES.md"
    let new_=[sprintf "#### %s" (version.ToString()); orig]
    File.WriteAllText("RELEASE_NOTES.md", toLines new_)
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "all" DoNothing
"clean"
  ==> "build"
  ==> "pack"
  ==> "push"

"clean"
  ==> "build"
  ==> "CopyBinaries"
  ==> "test"
  ==> "all"

"build"
  ==> "bump"

RunTargetOrDefault "test"
