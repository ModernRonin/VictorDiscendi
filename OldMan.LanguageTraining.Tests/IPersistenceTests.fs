(*
    These tests should be true for any type implementing IPersistence.
    If at some point we get another implementation, find out how
    to parameterize an FsCheck test suite on the underTest type
*)

module OldMan.LanguageTraining.Tests.Persistence.IPersistence

open System
open OldMan.LanguageTraining.Persistence
open OldMan.LanguageTraining.Domain

open FsCheck
open FsCheck.NUnit



type BackStore()= 
    let mutable data: Map<DataKind, string>= Map.empty
    member this.Load kind= data |> Map.find kind
    member this.Save kind what= data <- data.Add (kind, what)

let createWithEmptyBackStore()= 
    let backStore= new BackStore()
    (new CsvPersistence(backStore.Load, backStore.Save)) :> IPersistence

// necessary because the CSV type provider does not deal correctly with "\n" in field values
//let isValidString (s:string) = null<>s && not (s.Contains("\n")) && not (s.Contains("\r"))

let charSet= "abc"
let isValidString (s:string) = null<>s && not (s |> Seq.exists (fun c -> not (charSet |> Seq.contains c)))
let createConfig left right = {LeftLanguageName= left; RightLanguageName= right}
let generateString= Arb.generate<string> |> Gen.filter isValidString
let generateConfig= createConfig <!> generateString <*> generateString 

type Generators =
  static member String() =
    { 
        new Arbitrary<string>() with
            override x.Generator = generateString
    }
  static member LanguageConfiguration()= 
    {
        new Arbitrary<LanguageConfiguration>() with
           override x.Generator = generateConfig
    }

Arb.register<Generators>() |> ignore

(* 
when I update config and read it, the result should be what I put in
when I add n pairs and then do GetPairs, the result should be what I added
when I add pairs X and Y, then update X for Z, then do GetPairs, I should get Y and Z
*)

module Configuration=
    let ``read returns values from last update`` config=
        let persistence= createWithEmptyBackStore()
        persistence.UpdateConfiguration(config) 
        persistence.GetConfiguration() = config

    let ``config does not contain null values`` config = config.LeftLanguageName<>null && config.RightLanguageName<>null

    //[<Property>]
    let ``read after update gets the values sent with update`` (config: LanguageConfiguration)=
        ``config does not contain null values`` config ==> ``read returns values from last update`` config
        
        
        

