module OldMan.LanguageTraining.Persistence

open System
open System.Globalization
open System.IO
open FSharp.Data
open Domain

// persistence types
type PersistentConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >

type private PersistentPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (DateTime), LastAsked (DateTime), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >

type private PersistentTag= 
    CsvProvider<
        Schema = "Id (int64), Tag (string)",
        HasHeaders=false
        >

type private PersistentTagWordAssociation=
    CsvProvider<
        Schema = "TagId (int64), PairId (uint64)",
        HasHeaders=false
        >

type private Database=
    {
        Configuration: PersistentConfiguration
        WordPairs: PersistentPair list
        Tags: PersistentTag list
        TagWordAssociations: PersistentTagWordAssociation list
    }

let private serializeCfg config = PersistentConfiguration.Row( config.LeftLanguageName, config.RightLanguageName)
let private deserializeCfg (config: PersistentConfiguration.Row) = {LeftLanguageName= config.LeftLanguageName; RightLanguageName= config.RightLanguageName}

let private serializeWord word = 
    match word with
    | None -> ""
    | Some x -> x

let private timeFormat= "yyyyMMddHHmmss"
let private serializeTimestamp (timeStamp: DateTime)= timeStamp.ToString(timeFormat, CultureInfo.InvariantCulture)
let private deserializeTimestamp asString= DateTime.ParseExact(asString, timeFormat, CultureInfo.InvariantCulture)

let extractWordTexts (pair: WordPair)= 
    let (left, right) = pair.Pair
    (serializeWord left, serializeWord right)

let createPair id pair= 
    let (left, right) = pair |> extractWordTexts
    PersistentPair.Row(id, 
                        left, 
                        right, 
                        pair.Created |> serializeTimestamp, 
                        pair.ScoreCard.LastAsked |> serializeTimestamp, 
                        int64 pair.ScoreCard.TimesAsked, 
                        pair.ScoreCard.LeftScore, 
                        pair.ScoreCard.RightScore)
(*
add Pair
    [x] creates PersistentPair
    creates new PersistentTags as needed
    creates new PersistentTagWordAssociation

update Pair
    finds corresponding Pair, updates words, lastasked, count, scores
    creates new PersistentTags as needed
    creates new PersistentTagWordAssociation as needed
    deletes PersistentTagWordAssociation as needed

load Pairs

*)


let private saveConfiguration (path: string) config=
    let csv= new PersistentConfiguration([serializeCfg config])
    csv.Save path

let private loadConfiguration (path: string) =
    let csv=  PersistentConfiguration.Load path
    csv.Rows |> Seq.head |> deserializeCfg
   
let private addPair (path: string) pair=
    let csv= PersistentPair.Load path
    let nextId= 1L + (csv.Rows |> Seq.map (fun r -> r.Id) |> Seq.max)
    pair |> (createPair nextId) |> Seq.singleton |> csv.Append



type Persistence(directory: string)=
    let configPath= Path.Combine(directory, "LanguageConfiguration.csv")
    let pairsPath= Path.Combine(directory, "Pairs.csv")
    member this.SaveConfiguration =  saveConfiguration configPath
    member this.LoadConfiguration =  loadConfiguration configPath
    member this.AddPair = addPair pairsPath

        
        


