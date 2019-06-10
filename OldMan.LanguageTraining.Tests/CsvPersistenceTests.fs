namespace OldMan.LanguageTraining.Tests.Persistence
module CsvPersistence=

    open System
    open NUnit.Framework
    open FsUnitTyped
    open OldMan.LanguageTraining.Persistence
    open OldMan.LanguageTraining.Domain
    
    let shouldNotHappen() = raise (AssertionException("Should not get here"))

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
        (new CsvPersistence(loader, save)) :> IPersistence

   
    [<Test>]
    let ``GetConfiguration loads from CSV``() =
        let underTest = create [(Configuration, "English,German")] 
        underTest.GetConfiguration() |> shouldEqual { LeftLanguageName="English"; RightLanguageName="German"}
        saveCalls |> shouldBeEmpty
        
    [<Test>]
    let ``UpdateConfiguration saves to CSV``()=
        let underTest= create []
        underTest.UpdateConfiguration { LeftLanguageName="English"; RightLanguageName="German"} 
        saveCalls |> shouldEqual (Map [(Configuration,"English,German\r\n")])

  
    [<Test>]
    let ``GetPairs loads word pairs with their tags``()=
        let underTest= create [
                                (Words, "13,apple,Apfel,20100307221005,20190401100000,3,2,-1\r
                                            17,car,Auto,20100307221107,20190402110000,4,3,-2")
                                (Tagging, "19,Obst\r\n23,Transport\r\n29,Substantiva\r\n31,Verba")
                                (WordTagAssociation, "19,13\r\n23,17\r\n29,13\r\n29,17")
                                ]
        let result= underTest.GetPairs() 
        result |> shouldEqual   [
                                    {
                                        Id= Id 13L
                                        Left= Word "apple"
                                        Right= Word "Apfel"
                                        Created= DateTime(2010, 3, 7, 22, 10, 5)
                                        Tags= [
                                                {
                                                    Id= Id.wrap 19L
                                                    Text="Obst"
                                                }; 
                                                {
                                                    Id= Id.wrap 29L
                                                    Text= "Substantiva"
                                                }
                                                ] 
                                        ScoreCard={
                                                        LastAsked= DateTime(2019,4,1,10, 0, 0)
                                                        TimesAsked= Count (uint32 3)
                                                        LeftScore= Score 2
                                                        RightScore= Score -1
                                                    }
                                    }
                                    {
                                        Id= Id 17L
                                        Left= Word  "car"
                                        Right=  Word "Auto"
                                        Created= DateTime(2010, 3, 7, 22, 11, 7)
                                        Tags= [
                                                {
                                                    Id= Id.wrap 23L
                                                    Text="Transport"
                                                }; 
                                                {
                                                    Id= Id.wrap 29L
                                                    Text= "Substantiva"
                                                }
                                                ] 
                                        ScoreCard={
                                                        LastAsked= DateTime(2019,4,2,11, 0, 0)
                                                        TimesAsked= Count (uint32 4)
                                                        LeftScore= Score 3
                                                        RightScore= Score -2
                                                    }
                                    }
                                ]
