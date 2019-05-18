module OldMan.LanguageTraining.Tests.Persistence

open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Persistence
open OldMan.LanguageTraining.Domain
    
let shouldNotHappen() = raise (AssertionException("Should not get here"))

type UnderTest= OldMan.LanguageTraining.Persistence.Persistence

let loader returnValues = 
    fun kind -> match returnValues |> Map.ofList |> Map.tryFind kind with
                | Some x -> x
                | _ -> shouldNotHappen()
    
let mutable saveCalls: Map<DataKind, string> = Map.empty

let save kind data = 
    saveCalls <- saveCalls.Add (kind, data)

let create loadResults =
    let loader= loader loadResults
    saveCalls <- Map.empty
    new UnderTest(loader, save)
    
module ``GetConfiguration``=
    [<Test>]
    let ``loads from CSV``() =
        let underTest = create [(Configuration, "English,German")] 
        underTest.GetConfiguration() |> shouldEqual { LeftLanguageName="English"; RightLanguageName="German"}
        saveCalls |> shouldBeEmpty
        
module ``UpdateConfiguration``=
    [<Test>]
    let ``saves to CSV``()=
        let underTest= create []
        underTest.UpdateConfiguration { LeftLanguageName="English"; RightLanguageName="German"} 
        saveCalls |> shouldEqual (Map [(Configuration,"English,German\r\n")])