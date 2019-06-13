namespace OldMan.LanguageTraining.Tests.DomainLogic

open System
open NUnit.Framework
open FsUnitTyped
open OldMan.LanguageTraining.Domain

module RawQuestion=
    [<Test>]
    let ``toReference creates a reference with the pair id and the side of the answer``()=
        {
            PairId= Id.wrap 13L
            Question= Word "apple"
            Answer= Word "Apfel"
            SideOfAnswer= Right
            Score= Score 3
            LastAsked= DateTime.Today
        } |> RawQuestion.toReference |> shouldEqual 
            {
                PairId= Id.wrap 13L
                Side= Right
            }

    module ``ofPair``=
        let pair= {
                    WordPair.create (Word "apple", Word "Apfel", []) with 
                        ScoreCard= {
                                        LeftScore= Score 13
                                        RightScore= Score 17
                                        LastAsked= DateTime.Today
                                        TimesAsked= Count 29u
                                   }
                        Id= Id.wrap 23L
                  }
        let result= RawQuestion.ofPair pair
        [<Test>]
        let ``returns 2 questions``()= result |> shouldHaveLength 2

        [<Test>]
        let ``one result is from left to right``()=
            result |> shouldContain {
                                       PairId= Id.wrap 23L
                                       Question= Word "apple"
                                       Answer= Word "Apfel"
                                       SideOfAnswer= Right
                                       Score= Score 13
                                       LastAsked= DateTime.Today
                                    }
        [<Test>]
        let ``the other result is from right to left``()=
            result |> shouldContain {
                                       PairId= Id.wrap 23L
                                       Question= Word "Apfel"
                                       Answer= Word "apple"
                                       SideOfAnswer= Left
                                       Score= Score 17
                                       LastAsked= DateTime.Today
                                    }
            


