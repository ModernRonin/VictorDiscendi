module OldMan.LanguageTraining.Tests.Persistence

open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Persistence
open OldMan.LanguageTraining.Domain
    
let shouldNotHappen() = raise (AssertionException("Should not get here"))

type UnderTest= OldMan.LanguageTraining.Persistence.Persistence

module ``GetConfiguration``=
    [<Test>]
    let ``loads from CSV``() =
        let loader kind= match kind with
            | Configuration -> "English,German"
            | _ -> shouldNotHappen()
        let saver _ _ = shouldNotHappen()

        let underTest= new UnderTest(loader, saver)
        underTest.GetConfiguration() |> shouldEqual { LeftLanguageName="English"; RightLanguageName="German"}
        
module ``UpdateConfiguration``=
    [<Test>]
    let ``saves to CSV``()=
        let loader _ = shouldNotHappen()
        let saver kind data = match kind with
            | Configuration -> data |> shouldEqual "English,German\r\n"
            | _ -> shouldNotHappen()

        let underTest= new UnderTest(loader, saver)
        underTest.UpdateConfiguration { LeftLanguageName="English"; RightLanguageName="German"} 