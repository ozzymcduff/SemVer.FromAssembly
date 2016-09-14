// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Magnitude.fs"
#load "Compare.fs"
#load "SurfaceArea.fs"

open SemVer.FromAssembly

#r "../SampleProject/bin/Debug/SampleProject.dll"

SurfaceArea.get (typeof<SampleProject.Class1>.Assembly)