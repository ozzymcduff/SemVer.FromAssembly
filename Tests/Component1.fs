namespace Tests
open SemVer.FromAssembly
open Helpers
module CompareTests=
    let sample1= [("SampleProject",
                                  [("Module",
                                     ["Int32 get_t()";
                                       "Boolean Equals(System.Object)"; "Int32 GetHashCode()";
                                       "System.Type GetType()"; "System.String ToString()"; "Int32 t"])])]



    let sample2= [("SampleProject",
                                  [("Module",
                                     ["Int32 get_transform()";
                                       "Boolean Equals(System.Object)"; "Int32 GetHashCode()";
                                       "System.Type GetType()"; "System.String ToString()"; "Int32 transform"])])]




    let s1 = transform sample1
    let s2 = transform sample2

    let diff= Compare.comparePackages s1 s2