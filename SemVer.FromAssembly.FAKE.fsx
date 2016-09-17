#r @"./SemVer.FromAssembly/bin/Debug/SemVer.FromAssembly.exe"
#r @"./packages/FAKE/tools/FakeLib.dll"

open Fake.VersionHelper
open SemVer.FromAssembly

let x = Magnitude.Major
let y = parseVersion("1.0.0.0")

match x with
| Magnitude.Major m-> { y with Major=y.Major+1 }
| Magnitude.Minor m-> { y with Minor=y.Minor+1 }
| Magnitude.Patch m-> { y with Patch=y.Patch+1 }
|> printfn "%A"