namespace SemVer.FromAssembly
open System
open Argu
open Chiron
open System.Reflection
open System.Diagnostics
open System.IO
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
type Result<'T,'TError> = 
         | Ok of 'T 
         | Error of 'TError
module Result =
    let map f inp = match inp with Error e -> Error e | Ok x -> Ok (f x)
    let bind f inp = match inp with Error e -> Error e | Ok x -> f x

module Program=
    let surfaceAreaCli file=
        use p = new Process()
        let isRunningMono = Type.GetType ("Mono.Runtime") <> null
        let pathToProgram = Assembly.GetExecutingAssembly().Location//, "SemVer.FromAssembly.exe")
        let cliArgs = sprintf "--surface-of '%s'" file
        let exe = if isRunningMono then "mono" else pathToProgram
        let args = if isRunningMono then sprintf "'%s' %s" pathToProgram cliArgs else cliArgs
        let st = ProcessStartInfo()
        st.CreateNoWindow <- true
        st.UseShellExecute <- false
        st.RedirectStandardOutput <- true
        st.RedirectStandardError <-true
        st.FileName <- exe
        st.WorkingDirectory <- Environment.CurrentDirectory
        st.Arguments <- args
        p.StartInfo <- st
        if p.Start() then
            p.WaitForExit()

            let error = p.StandardError.ReadToEnd()
            let output = p.StandardOutput.ReadToEnd()
            match p.ExitCode, String.IsNullOrWhiteSpace output with
            | 0,false->
                output
                |> Json.parse
                |> Json.deserialize
                |> Ok
            | _,_->
                Error error
        else
            Error "Couldn't start process"

    let getDiff original new_=
        let maybeOriginal,maybeNew_= surfaceAreaCli original, surfaceAreaCli new_
        match maybeOriginal,maybeNew_ with
        | Ok original, Ok new_ ->

            let changes =  Compare.comparePackages original new_
            changes
            |> Ok
        | _, _ ->
            let errors = 
                [maybeOriginal;maybeNew_] 
                |> List.choose (fun r-> match r with Error e-> Some e | _ -> None)
                |> List.toArray
            
            Error (String.Join(Environment.NewLine, errors) )
    let writeResult (res:Result<string,string>)=
        match res with
        | Ok msg-> Console.WriteLine msg ; 0
        | Error msg->Console.Error.WriteLine msg ; 1


    [<EntryPoint>]
    let main argv = 
        let parser = ArgumentParser.Create<CLIArguments>(programName = "SemVer.FromAssembly.exe")

        let results = parser.Parse argv

        let all = results.GetAllResults()
        if List.isEmpty all || results.IsUsageRequested then
            Error(parser.PrintUsage())
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
                |> Ok
            | None, Some ( original,new_), None ->
                getDiff original new_
                |> Result.map (fun diff->
                    diff
                    |> Json.serialize
                    |> Json.formatWith JsonFormattingOptions.Pretty
                    ) 
            | None, None, Some (original,new_) ->
                getDiff original new_
                |> Result.map (fun diff->
                    let magnitude = Compare.packageChangeMagnitude diff
                    magnitude.ToString()
                    ) 
             | _, _,_ ->
                Error(parser.PrintUsage())
        |> writeResult