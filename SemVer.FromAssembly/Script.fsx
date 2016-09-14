// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

open SemVer.FromAssembly

#r "../SampleProject/bin/Debug/SampleProject.dll"

#load "Magnitude.fs"
#load "Compare.fs"
#load "SurfaceArea.fs"


printfn "%O" (SurfaceArea.get (typeof<SampleProject.Class1>.Assembly))

(*
let sample2= [|("SampleProject",
                              [|("Class1",
                                 [|"System.String get_X()"; "Boolean Equals(System.Object)";
                                   "Int32 GetHashCode()"; "System.Type GetType()";
                                   "System.String ToString()"; "Void .ctor()"; "System.String X"|]);
                                ("Module",
                                 [|"Int32 get_t()"; "SampleProject.Class1 get_y()";
                                   "Boolean Equals(System.Object)"; "Int32 GetHashCode()";
                                   "System.Type GetType()"; "System.String ToString()"; "Int32 t";
                                   "SampleProject.Class1 y"|])|]);
                             (null,
                              [|("Script",
                                 [|"Boolean Equals(System.Object)"; "Int32 GetHashCode()";
                                   "System.Type GetType()"; "System.String ToString()"|])|])|]

                                   *)