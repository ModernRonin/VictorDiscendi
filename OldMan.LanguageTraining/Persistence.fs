module OldMan.LanguageTraining.Persistence

open System
open System.Globalization
open System.IO
open FSharp.Data
open Domain

// persistence types
type private PersistentConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >
let private serializeCfg config = PersistentConfiguration.Row( config.LeftLanguageName, config.RightLanguageName)
let private deserializeCfg (config: PersistentConfiguration.Row) = {LeftLanguageName= config.LeftLanguageName; RightLanguageName= config.RightLanguageName}

type private PersistentPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (string), LastAsked (string), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >
let private serializeWord word = 
    match word with
    | None -> ""
    | Some x -> x

let private deserialize word =
    match word with
    | "" -> None
    | x -> Some x

let private timeFormat= "yyyyMMddHHmmss"
let private serializeTimestamp (timeStamp: DateTime)= timeStamp.ToString(timeFormat, CultureInfo.InvariantCulture)
let private deserializeTimestamp asString= DateTime.ParseExact(asString, timeFormat, CultureInfo.InvariantCulture)

let private extractWordTexts (pair: WordPair)= 
    let (left, right) = pair.Pair
    (serializeWord left, serializeWord right)

let private toWordPair left right= (deserialize left, deserialize right)
    
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

let private loadPair tagsLoader (pair: PersistentPair.Row) =
    {
        Pair= toWordPair pair.Left pair.Right
        Created = pair.Created |> deserializeTimestamp
        Tags = tagsLoader pair.Id
        ScoreCard= 
            {
                LastAsked=  pair.LastAsked |> deserializeTimestamp
                TimesAsked= uint32 pair.TimesAsked
                LeftScore= pair.LeftScore
                RightScore= pair.RightScore
            }
    }

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


// helpers
let inline private idOf (x: ^R)=
    (^R: (member Id: int64) (x))


let inline private nextIdIn (rows: ^record seq)= 
    1L + (rows |> Seq.map idOf |> Seq.max)
    

// public interface
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

    let editPair pair=
        let csv= PersistentPair.Load pairsPath
        let (left, right)= extractWordTexts pair
        let oldPair= csv.Rows |> Seq.find (fun p -> p.Left=left && p.Right=right)
        let newPair= makePair oldPair.Id pair
        csv.Filter((fun p -> p.Id<>oldPair.Id)).Append(newPair |> Seq.singleton).Save pairsPath
        oldPair.Id

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
        // TODO: for removed tags, check if they are in use by anything else, if no delete them
        let csv= nu |> PersistentTagPairAssociation.Load(associationsPath).Filter((fun a -> a.PairId<>pairId)).Append
        csv.Save associationsPath

    let getAssociatedTags pairId=
        let tagIds= PersistentTagPairAssociation.Load(associationsPath).Filter((fun a -> a.PairId=pairId)).Rows 
                        |> Seq.map (fun a -> a.TagId) |> Set.ofSeq
        PersistentTag.Load(tagsPath).Filter((fun t -> tagIds.Contains t.Id)).Rows 
                        |> Seq.map (fun t -> t.Tag) |> List.ofSeq

    member this.UpdateConfiguration config=  
        let csv= new PersistentConfiguration([serializeCfg config])
        csv.Save configPath
        
    member this.GetConfiguration =  
        let csv=  PersistentConfiguration.Load configPath
        csv.Rows |> Seq.head |> deserializeCfg

    member this.AddPair pair = 
        let pairId= createPair pair
        let tagIds= getTagIds pair.Tags
        updateAssociations pairId tagIds

    member this.UpdatePair pair=
        let pairId= editPair pair
        let tagIds= getTagIds pair.Tags
        updateAssociations pairId tagIds
  
    member this.GetPairs =
        let csv= PersistentPair.Load pairsPath
        csv.Rows |> Seq.map (loadPair getAssociatedTags)
        
