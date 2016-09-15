namespace Tests
open SemVer.FromAssembly
open Helpers
open NUnit.Framework
[<TestFixture>]
type CompareTests()=
    let sample1= [("SampleProject",
                                  [("Module",
                                     ["Int32 get_t()"])])]

    let sample_with_t_removed= [("SampleProject",
                                  [("Module",
                                     [])])]


    let sample_with_t_renamed= [("SampleProject",
                                  [("Module",
                                     ["Int32 get_transform()"])])]

    let sample_with_n_added= [("SampleProject",
                                  [("Module",
                                     ["Int32 get_t()";
                                     "System.String GetCode()"])])]

    let getMagnitude a b=
        let s1 = transform a
        let s2 = transform b

        let diff= Compare.comparePackages s1 s2
        Compare.packageChangeMagnitude diff

    [<Test>]
    member this.``with a variable renamed``() = 
        let magnitude = getMagnitude sample1 sample_with_t_renamed
        Assert.AreEqual(Magnitude.Major, magnitude)
    
    [<Test>]
    member this.``with a variable added``() = 
        let magnitude = getMagnitude sample1 sample_with_n_added
        Assert.AreEqual(Magnitude.Minor, magnitude)

    [<Test>]
    member this.``with a variable removed``() = 
        let magnitude = getMagnitude sample1 sample_with_t_removed
        Assert.AreEqual(Magnitude.Major, magnitude)
         
    [<Test>]
    member this.``when no visible change``() = 
        let magnitude = getMagnitude sample1 sample1
        Assert.AreEqual(Magnitude.Patch, magnitude)    