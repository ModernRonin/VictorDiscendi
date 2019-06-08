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

let stringGenerator= 
    Arb.Default.Char().Generator |> 
    Gen.filter (fun c -> Char.IsLetterOrDigit(c) || Char.IsPunctuation(c)) |> 
    Gen.nonEmptyListOf |> Gen.map Array.ofList |> Gen.map (fun c -> new string(c))

let createConfig left right = {LeftLanguageName= left; RightLanguageName= right}

type Generators =
  static member String()=
    { 
        new Arbitrary<string>() with
            override x.Generator = stringGenerator
    }
  static member LanguageConfiguration()= 
    {
        new Arbitrary<LanguageConfiguration>() with
           override x.Generator = createConfig <!> stringGenerator <*> stringGenerator
    }



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

    [<Property(Arbitrary= [| typeof<Generators> |])>]
    let ``read after update gets the values sent with update`` (config: LanguageConfiguration)=
        ``read returns values from last update`` config
        
        
        

