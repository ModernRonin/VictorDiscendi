module OldMan.LanguageTraining.Tests.Persistence

open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Persistence
open OldMan.LanguageTraining.Domain
    
module ``GetConfiguration``=
    type UnderTest= OldMan.LanguageTraining.Persistence.Persistence
    [<Test>]
    let ``loads from CSV``() =
        let loader kind= match kind with
            | Configuration -> "English,German"
            | _ -> raise (AssertionException("loads wrong data type"))
        let saver kind data = raise (AssertionException("tries to save"))

        let underTest= new UnderTest(loader, saver)
        underTest.GetConfiguration() |> shouldEqual { LeftLanguageName="English"; RightLanguageName="German"}
        