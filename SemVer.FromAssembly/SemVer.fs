namespace SemVer.FromAssembly
open System
open Argu
open Chiron
open SynVer
open System.Reflection
open System.IO
type CLIArguments =
    | Surface_of of path:string
    | Output of path:string
    | Magnitude of original:string * ``new``:string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Surface_of _ -> "Get the public api surface of the .net binary as json"
            | Magnitude _-> "Get the magnitude of the difference between two .net binaries"
            | Output _-> "Send output to file"
module SemVer=
    let surfaceAreaCli file : Result<Package,string>=
        if File.Exists file then
            let exe = typeof<CLIArguments>.Assembly.Location
            let tmp = Path.GetTempFileName()
            try
            let args = sprintf "--surface-of '%s' --output '%s'" file tmp
            Process.executeDotnetExe exe args
            |> Result.map 
                (fun output->
                    File.ReadAllText tmp
                    |> Json.parse
                    |> Json.deserialize
                )
            finally
            if File.Exists tmp then File.Delete tmp 
        else
            Result.Error (sprintf "Could not find file %s " file)

    let getDiff original new_ : Result<string,string>=
        let maybeOriginal,maybeNew_= surfaceAreaCli original, surfaceAreaCli new_
        match maybeOriginal,maybeNew_ with
        | Ok original, Ok new_ ->

            let changes =  SurfaceArea.diff original new_
            String.Join(Environment.NewLine, changes)
            |> Ok
        | _, _ ->
            let errors = 
                [maybeOriginal;maybeNew_] 
                |> List.choose (fun r-> match r with Result.Error e-> Some e | _ -> None)
                |> List.toArray
            
            Result.Error (String.Join(Environment.NewLine, errors) )
    let getMagnitude verNr original new_ : Result<string,string>=
        let maybeOriginal,maybeNew_= surfaceAreaCli original, surfaceAreaCli new_
        match maybeOriginal,maybeNew_ with
        | Ok original, Ok new_ ->

            let (version,magnitude) =  SurfaceArea.bump verNr original new_
            magnitude.ToString()
            |> Ok
        | _, _ ->
            let errors = 
                [maybeOriginal;maybeNew_] 
                |> List.choose (fun r-> match r with Result.Error e-> Some e | _ -> None)
                |> List.toArray
            
            Result.Error (String.Join(Environment.NewLine, errors) )


    [<EntryPoint>]
    let main argv = 
        let parser = ArgumentParser.Create<CLIArguments>(programName = "SemVer.FromAssembly.exe")

        let results = parser.Parse argv
        let writeResult (res:Result<string,string>)=
            match res with
            | Ok msg-> Console.WriteLine msg ; 0
            | Result.Error msg->Console.Error.WriteLine msg ; 1

        let all = results.GetAllResults()
        if List.isEmpty all then
            Result.Error(parser.PrintUsage())
        elif results.IsUsageRequested then
            Ok(parser.PrintUsage())
        else
            let maybeFile = results.TryGetResult(<@ Surface_of @>)
            //let maybeDiff = results.TryGetResult(<@ Diff @>)
            let maybeMagnitude = results.TryGetResult(<@ Magnitude @>)
            let maybeOutput = results.TryGetResult(<@ Output @>)
            match maybeFile, (*maybeDiff,*) maybeMagnitude with
            | Some file, None ->
                let assembly = Assembly.LoadFrom(file)
                (SurfaceArea.ofAssembly assembly)
                |> Json.serialize
                |> Json.formatWith JsonFormattingOptions.Pretty
                |> Result.Ok
            | None, Some (original,new_) ->
                getMagnitude "" original new_
            | _,_ ->
                Result.Error(parser.PrintUsage())
            |> (fun res-> 
                match res, maybeOutput with
                | Result.Ok content, Some output-> 
                    File.WriteAllText(output, content)
                    res
                | _ -> res
                )
        |> writeResult