module OldMan.LanguageTraining.Persistence

open System
open System.Globalization
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
        Id = pair.Id
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
    


type IPersistence=
    abstract member UpdateConfiguration: LanguageConfiguration->unit
    abstract member GetConfiguration : unit->LanguageConfiguration
    abstract member AddPair: WordPair -> WordPair
    abstract member UpdatePair: int64 -> WordPair -> unit
    abstract member GetPairs: unit -> WordPair list

type DataKind=
    | Configuration
    | Words
    | Tagging
    | WordTagAssociation

type Loader= DataKind -> string
type Saver= DataKind -> string -> unit

type CsvPersistence(loader: Loader, saver: Saver)=
    let loadWords()= Words |> loader |> PersistentPair.ParseRows |> List.ofArray
    let loadTags()= Tagging |> loader |> PersistentTag.ParseRows |> List.ofArray
    let loadAssociations()= WordTagAssociation |> loader |> PersistentTagPairAssociation.ParseRows |> List.ofArray
    let saveWords w= ((new PersistentPair(w)).SaveToString()) |> saver Words 
    let saveTags t= ((new PersistentTag(t)).SaveToString()) |> saver Tagging
    let saveAssociations a= ((new PersistentTagPairAssociation(a)).SaveToString()) |> saver WordTagAssociation
    let loadConfig()= 
        match Configuration |> loader |> PersistentConfiguration.ParseRows |> List.ofArray with
        | [] -> PersistentConfiguration.Row("", "")
        | head::_ -> head

    let saveConfig c= ((new PersistentConfiguration(c |> Seq.singleton )).SaveToString()) |> saver Configuration

    let createPair pair= 
        let existing= loadWords()
        let nextId=  nextIdIn existing 
        let result= pair |> (makePair nextId) 
        let updated= result |> List.singleton |> List.append existing
        saveWords updated
        result

    let editPair id nu=
        let newRecord= makePair id nu
        let existingWithoutOld= loadWords() |> List.filter (fun p -> p.Id<>id)
        newRecord :: existingWithoutOld |> saveWords

    let getTagIds tags=
        let existing= loadTags()
        let nextId= nextIdIn existing
        let existingTagsOnly= existing |> Seq.map (fun t -> t.Tag) |> Set.ofSeq
        let updated= tags |> Set.ofSeq |> Set.difference existingTagsOnly |> List.ofSeq |> List.indexed 
                     |> List.map (fun (i, t) -> makeTag (nextId+int64 i) t) |> List.append existing
        saveTags updated
        updated |> List.map idOf

    let updateAssociations pairId tagIds=
        let nu= tagIds |> List.map (fun tagId -> makePairTagAssociation pairId tagId)
        // TODO: for removed tags, check if they are in use by anything else, if no delete them
        let toKeep= loadAssociations() |> List.filter (fun a -> a.PairId<>pairId)
        nu |> List.append <| toKeep |> saveAssociations

    let getAssociatedTags pairId=
        let tagIds= loadAssociations() |> List.filter (fun a -> a.PairId=pairId) |> List.map (fun a -> a.TagId) |> Set.ofList
        loadTags() |> List.filter (fun t -> tagIds.Contains t.Id) |> List.map (fun t -> t.Tag)

    let deserializePair = loadPair getAssociatedTags

    interface IPersistence with 
        member this.UpdateConfiguration config=  config |> serializeCfg |> saveConfig 
        
        member this.GetConfiguration() =  loadConfig() |> deserializeCfg

        member this.AddPair pair = 
            let result= createPair pair
            let tagIds= getTagIds pair.Tags
            updateAssociations result.Id tagIds
            result |> deserializePair

        member this.UpdatePair id newPair=
            editPair id newPair
            let tagIds= getTagIds newPair.Tags
            updateAssociations id tagIds
  
        member this.GetPairs() = loadWords() |> List.map deserializePair
        
