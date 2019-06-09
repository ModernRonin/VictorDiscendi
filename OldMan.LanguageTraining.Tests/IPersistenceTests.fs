(*
    These tests should be true for any type implementing IPersistence.
    If at some point we get another implementation, find out how
    to parameterize an FsCheck test suite on the underTest type
*)

namespace OldMan.LanguageTraining.Tests.Persistence
module Setup=
    open System
    open OldMan.LanguageTraining.Persistence
    open OldMan.LanguageTraining.Domain

    open FsCheck

    type BackStore()= 
        let mutable data: Map<DataKind, string>= Map.empty.Add(DataKind.Configuration,"").Add(DataKind.Tagging, "").Add(DataKind.Words, "").Add(DataKind.WordTagAssociation, "")
        member this.Load kind= data |> Map.find kind
        member this.Save kind what= data <- data.Add (kind, what)

    let createWithEmptyBackStore()= 
        let backStore= new BackStore()
        (new CsvPersistence(backStore.Load, backStore.Save)) :> IPersistence

    let stringGenerator= 
        Arb.Default.Char().Generator |> 
        Gen.filter (fun c -> Char.IsLetterOrDigit(c) || Char.IsPunctuation(c)) |> 
        Gen.nonEmptyListOf |> Gen.map Array.ofList |> Gen.map (fun c -> new string(c))

    type Generators=
        static member String()= 
            {
                new Arbitrary<string>() with 
                    override x.Generator= stringGenerator
            }
        static member Id()=
            {
                new Arbitrary<Id>() with 
                    override x.Generator= Gen.constant Id.uninitialized
            }


(* 

when I add pairs X and Y, then update X for Z, then do GetPairs, I should get Y and Z
*)

open FsCheck;
open FsCheck.Xunit
open FsUnitTyped
open NUnit.Framework
open OldMan.LanguageTraining.Domain
open Setup

type FsProperty= FsCheck.Xunit.PropertyAttribute

[<Properties(Arbitrary= [| typeof<Generators> |])>]
module IPersistence=
    [<FsProperty>]
    let ``getting the configuration after updating it returns what it was updated to`` 
        (config: LanguageConfiguration)=
        let persistence= createWithEmptyBackStore()
        persistence.UpdateConfiguration(config) 
        persistence.GetConfiguration() = config

    [<FsProperty>]
    let ``GetPairs() returns the combined return values of all prior calls to AddPair``
        (pairs: WordPair list)=
        let persistence= createWithEmptyBackStore()
        let expected= pairs |> List.map persistence.AddPair
        persistence.GetPairs() = expected

    [<FsProperty>]
    let ``AddPair() returns a pair with Id set``
        (pair: WordPair)=
        let persistence= createWithEmptyBackStore()
        let result= persistence.AddPair pair
        result.Id <> pair.Id

    [<FsProperty>]
    let ``all pairs added with AddPair() have different Ids``
        (pairs: WordPair list)=
        let persistence= createWithEmptyBackStore()
        let ids= pairs |> List.map persistence.AddPair |> List.map (fun p -> p.Id)
        ids |> List.groupBy id |> List.forall (fun (_, l) -> l.Length=1)

    [<Test>]
    let ``AddPair() X and Y followed by UpdatePair X to Z followed by GetPairs() returns Z and Y``()=
        let persistence= createWithEmptyBackStore()
        let generator= Arb.generate<WordPair>
        let x= Gen.sample 1 1 generator |> List.head
        let y= Gen.sample 1 1 generator |> List.head
        let addedX= persistence.AddPair x
        let addedY= persistence.AddPair y
        let z= {addedX with Left=Word "alpha"; Right=Word "bravo"}
        persistence.UpdatePair z
        let allPairs= persistence.GetPairs() 
        allPairs |> shouldHaveLength 2
        allPairs |> shouldContain addedY
        allPairs |> shouldContain z


        
        

