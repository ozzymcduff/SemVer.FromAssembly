namespace SemVer.FromAssembly
open System
open Argu
open Chiron
open System.Reflection
type CLIArguments =
    | Surface_of of path:string
    | Diff of original:string * ``new``:string
    | Magnitude of original:string * ``new``:string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Surface_of _ -> "Get the public api surface of the .net binary as json"
            | Diff _ -> "Get the difference between two .net binaries as json"
            | Magnitude _-> "Get the magnitude of the difference between two .net binaries"
module SemVer=
    let surfaceAreaCli file : Result<Package,string>=
        let exe = typeof<CLIArguments>.Assembly.Location
        let args = sprintf "--surface-of '%s'" file
        Process.executeDotnetExe exe args
        |> Result.map 
            (fun output->
                output
                |> Json.parse
                |> Json.deserialize
            )

    let getDiff original new_ : Result<PackageChanges,string>=
        let maybeOriginal,maybeNew_= surfaceAreaCli original, surfaceAreaCli new_
        match maybeOriginal,maybeNew_ with
        | Ok original, Ok new_ ->

            let changes =  Compare.comparePackages original new_
            changes
            |> Ok
        | _, _ ->
            let errors = 
                [maybeOriginal;maybeNew_] 
                |> List.choose (fun r-> match r with Result.Error e-> Some e | _ -> None)
                |> List.toArray
            
            Result.Error (String.Join(Environment.NewLine, errors) )
    let getMagnitude original new_ : Result<Magnitude,string>=
            getDiff original new_
            |> Result.map Compare.packageChangeMagnitude 


    let writeResult (res:Result<string,string>)=
        match res with
        | Ok msg-> Console.WriteLine msg ; 0
        | Result.Error msg->Console.Error.WriteLine msg ; 1


    [<EntryPoint>]
    let main argv = 
        let parser = ArgumentParser.Create<CLIArguments>(programName = "SemVer.FromAssembly.exe")

        let results = parser.Parse argv

        let all = results.GetAllResults()
        if List.isEmpty all || results.IsUsageRequested then
            Result.Error(parser.PrintUsage())
        else
            let maybeFile = results.TryGetResult(<@ Surface_of @>)
            let maybeDiff = results.TryGetResult(<@ Diff @>)
            let maybeMagnitude = results.TryGetResult(<@ Magnitude @>)
            match maybeFile, maybeDiff, maybeMagnitude with
            | Some file, None, None ->
                let assembly = Assembly.LoadFrom(file)
                (SurfaceArea.get assembly)
                |> Json.serialize
                |> Json.formatWith JsonFormattingOptions.Pretty
                |> Result.Ok
            | None, Some ( original,new_), None ->
                getDiff original new_
                |> Result.map (fun diff->
                    diff
                    |> Json.serialize
                    |> Json.formatWith JsonFormattingOptions.Pretty
                    ) 
            | None, None, Some (original,new_) ->
                getMagnitude original new_
                |> Result.map (fun m-> m.ToString()) 
             | _, _,_ ->
                Result.Error(parser.PrintUsage())
        |> writeResult