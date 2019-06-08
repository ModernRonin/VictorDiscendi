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

open FsCheck.Xunit
open OldMan.LanguageTraining.Domain
open Setup

[<Properties(Arbitrary= [| typeof<Generators> |])>]
module IPersistence=
    [<Property>]
    let ``getting the configuration after updating it returns what it was updated to`` 
        (config: LanguageConfiguration)=
        let persistence= createWithEmptyBackStore()
        persistence.UpdateConfiguration(config) 
        persistence.GetConfiguration() = config

    [<Property>]
    let ``GetPairs() returns the combined return values of all prior calls to AddPair``
        (pairs: WordPair list)=
        let persistence= createWithEmptyBackStore()
        let expected= pairs |> List.map persistence.AddPair
        persistence.GetPairs() = expected

    [<Property>]
    let ``AddPair() returns a pair with Id set``
        (pair: WordPair)=
        let persistence= createWithEmptyBackStore()
        let result= persistence.AddPair pair
        result.Id <> pair.Id


        
        

