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
let private serializeCfg config = PersistentConfiguration.Row( config.LeftLanguageName, config.RightLanguageName)
let private deserializeCfg (config: PersistentConfiguration.Row) = {LeftLanguageName= config.LeftLanguageName; RightLanguageName= config.RightLanguageName}

type private PersistentPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (DateTime), LastAsked (DateTime), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >
let private serializeWord word = 
    match word with
    | None -> ""
    | Some x -> x

let private timeFormat= "yyyyMMddHHmmss"
let private serializeTimestamp (timeStamp: DateTime)= timeStamp.ToString(timeFormat, CultureInfo.InvariantCulture)
let private deserializeTimestamp asString= DateTime.ParseExact(asString, timeFormat, CultureInfo.InvariantCulture)

let private extractWordTexts (pair: WordPair)= 
    let (left, right) = pair.Pair
    (serializeWord left, serializeWord right)

let private makePair id pair= 
    let (left, right) = pair |> extractWordTexts
    PersistentPair.Row(id, 
                        left, 
                        right, 
                        pair.Created |> serializeTimestamp, 
                        pair.ScoreCard.LastAsked |> serializeTimestamp, 
                        int64 pair.ScoreCard.TimesAsked, 
                        pair.ScoreCard.LeftScore, 
                        pair.ScoreCard.RightScore)

type private PersistentTag= 
    CsvProvider<
        Schema = "Id (int64), Tag (string)",
        HasHeaders=false
        >

let private makeTag id tag= PersistentTag.Row(id, tag)

type private PersistentTagPairAssociation=
    CsvProvider<
        Schema = "TagId (int64), PairId (int64)",
        HasHeaders=false
        >
let private makePairTagAssociation pairId tagId= PersistentTagPairAssociation.Row (tagId, pairId)

type private Database=
    {
        Configuration: PersistentConfiguration
        WordPairs: PersistentPair list
        Tags: PersistentTag list
        TagWordAssociations: PersistentTagPairAssociation list
    }


(*
update Pair
    finds corresponding Pair, updates words, lastasked, count, scores
    creates new PersistentTags as needed
    creates new PersistentTagWordAssociation as needed
    deletes PersistentTagWordAssociation as needed

load Pairs

*)

let inline idOf (x: ^R)=
    (^R: (member Id: int64) (x))


let inline private nextIdIn (rows: ^record seq)= 
    1L + (rows |> Seq.map idOf |> Seq.max)
    
type Persistence(directory: string)=
    let configPath= Path.Combine(directory, "LanguageConfiguration.csv")
    let pairsPath= Path.Combine(directory, "Pairs.csv")
    let tagsPath= Path.Combine(directory, "Tags.csv")
    let associationsPath= Path.Combine(directory, "PairTagAssociations.csv")

    let createPair pair= 
        let csv= PersistentPair.Load pairsPath
        let nextId=  nextIdIn csv.Rows 
        let updated= pair |> (makePair nextId) |> Seq.singleton |> csv.Append
        updated.Save pairsPath
        nextId

    let getTagIds tags=
        let csv= PersistentTag.Load tagsPath
        let nextId= nextIdIn csv.Rows
        let existing= csv.Rows |> Seq.map (fun t -> t.Tag) |> Set.ofSeq
        let updated= tags |> Set.ofSeq |> Set.difference existing |> Seq.indexed 
                     |> Seq.map (fun (i, t) -> makeTag (nextId+int64 i) t) |> csv.Append
        updated.Save tagsPath
        updated.Rows |> Seq.map idOf

    let updateAssociations pairId tagIds=
        let nu= tagIds |> Seq.map (fun tagId -> makePairTagAssociation pairId tagId)
        let csv= nu |> PersistentTagPairAssociation.Load(associationsPath).Filter((fun a -> a.PairId<>pairId)).Append
        csv.Save associationsPath
        
    member this.SaveConfiguration config=  
        let csv= new PersistentConfiguration([serializeCfg config])
        csv.Save configPath
        
    member this.LoadConfiguration =  
        let csv=  PersistentConfiguration.Load configPath
        csv.Rows |> Seq.head |> deserializeCfg

    member this.AddPair pair = 
        let pairId= createPair pair
        let tagIds= getTagIds pair.Tags
        updateAssociations pairId tagIds
        
        


