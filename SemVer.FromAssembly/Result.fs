namespace SemVer.FromAssembly
/// Will be obsolete once F# Core 4.1 is released
type Result<'T,'TError> = 
         | Ok of 'T 
         | Error of 'TError
module Result =
    let map f inp = match inp with Error e -> Error e | Ok x -> Ok (f x)
    let mapError f inp = match inp with Error e -> Error (f e) | Ok x -> Ok x
    let bind f inp = match inp with Error e -> Error e | Ok x -> f x
