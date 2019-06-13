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

//module 
