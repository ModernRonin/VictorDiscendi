module OldMan.LanguageTraining.Persistence

open System
open System.Globalization
open FSharp.Data
open Domain

// CSV Types:
type private PersistentConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >

type private PersistentPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (string), LastAsked (string), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >

type private PersistentTag= 
    CsvProvider<
        Schema = "Id (int64), Tag (string)",
        HasHeaders=false
        >

type private PersistentTagPairAssociation=
    CsvProvider<
        Schema = "TagId (int64), PairId (int64)",
        HasHeaders=false
        >

// serialization extensions
type private Count with
    member this.Serialize()= 
        match this with
        | Count x -> int64 x

    static member Deserialize (from: int64)=
        match from with 
        | x when int64 0<=x -> Count(uint32 x)
        | _ -> raise  (ArgumentOutOfRangeException ("count"))

type private Score with 
    member this.Serialize()=
        match this with 
        | Score x -> x
    static member Deserialize (from: int)= Score from

type private Id with 
    member this.Serialize()= 
        match this with 
        | Id x -> x
    static member Deserialize (from: int64)= Id from
    member this.AddDelta (delta: int)= Id.Deserialize (this.Serialize() + int64 delta)

type private Word with
    member this.Serialize()=
        match this with
        | Word None -> ""
        | Word (Some x) -> x
    static member Deserialize (from: string)= Word (Some from)

let private timeFormat= "yyyyMMddHHmmss"
type private DateTime with
    member this.Serialize()= this.ToString(timeFormat, CultureInfo.InvariantCulture)
    static member Deserialize (from: string)= DateTime.ParseExact(from, timeFormat, CultureInfo.InvariantCulture)

type private LanguageConfiguration with
    member this.Serialize()= PersistentConfiguration.Row (this.LeftLanguageName, this.RightLanguageName)
    static member Deserialize (from: PersistentConfiguration.Row)= {LeftLanguageName= from.LeftLanguageName; RightLanguageName= from.RightLanguageName}

type private WordPair with 
    member this.ExtractWords()=
        let (left, right) = this.Pair
        (left.Serialize(), right.Serialize())
    member this.Serialize() = 
        let (left, right) = this.ExtractWords()

        PersistentPair.Row( this.Id.Serialize(), 
                            left, 
                            right, 
                            this.Created.Serialize(), 
                            this.ScoreCard.LastAsked.Serialize(), 
                            this.ScoreCard.TimesAsked.Serialize(),
                            this.ScoreCard.LeftScore.Serialize(), 
                            this.ScoreCard.RightScore.Serialize())

    static member Deserialize tagsLoader (pair: PersistentPair.Row) =
        {
            Id = pair.Id |> Id.Deserialize
            Pair= (pair.Left |> Word.Deserialize, pair.Right |> Word.Deserialize)
            Created = pair.Created |> DateTime.Deserialize
            Tags = tagsLoader pair.Id
            ScoreCard= 
                {
                    LastAsked=  pair.LastAsked |> DateTime.Deserialize
                    TimesAsked= pair.TimesAsked |> Count.Deserialize
                    LeftScore= pair.LeftScore |> Score.Deserialize
                    RightScore= pair.RightScore |> Score.Deserialize
                }
        }

type private Tag with
    member this.Serialize() = PersistentTag.Row(this.Id.Serialize(), this.Text)
    static member Deserialize (from: PersistentTag.Row) = {Id= from.Id |> Id.Deserialize; Text= from.Tag}

let private serializePairTagAssociation pairId tagId= PersistentTagPairAssociation.Row (tagId, pairId)


// helpers
let inline private idOf (x: ^R)=
    (^R: (member Id: int64) (x))
        
        
let inline private nextIdIn (rows: ^record seq)= 
    Id (1L + (rows |> Seq.map idOf |> Seq.max))
        
    


type IPersistence=
    abstract member UpdateConfiguration: LanguageConfiguration->unit
    abstract member GetConfiguration : unit->LanguageConfiguration
    abstract member AddPair: WordPair -> WordPair
    abstract member UpdatePair: WordPair -> unit
    abstract member GetPairs: unit -> WordPair list
    abstract member GetTags: unit -> Tag list
    abstract member UpdateTag: Tag -> unit

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

    let createPair (pair: WordPair)= 
        let existing= loadWords()
        let nextId=  nextIdIn existing
        let result= {pair with Id=nextId}.Serialize()
        let updated= result |> List.singleton |> List.append existing
        saveWords updated
        result

    let editPair (nu: WordPair)=
        let newRecord= nu.Serialize()
        let existingWithoutOld= loadWords() |> List.filter (fun p -> p.Id<>nu.Id.Serialize())
        newRecord :: existingWithoutOld |> saveWords
        
    let getTagIds (tags: Tag list)=
        let existing= loadTags() 
        let doesNotExistYet (t:Tag) = existing |> List.exists (fun e -> e.Id=t.Id.Serialize()) |> not
        let newTags= tags |> List.filter doesNotExistYet
        let nextId= nextIdIn existing
        let assignNextId (index, tag): Tag = {tag with Id=nextId.AddDelta index}
        let newTagsWithIds= newTags |> List.indexed |> List.map assignNextId
        let updated= newTagsWithIds |> List.map (fun t -> t.Serialize()) |> List.append existing
        saveTags updated
        updated |> List.map idOf

    let updateAssociations pairId tagIds=
        let nu= tagIds |> List.map (fun tagId -> serializePairTagAssociation pairId tagId)
        // TODO: for removed tags, check if they are in use by anything else, if no delete them
        let toKeep= loadAssociations() |> List.filter (fun a -> a.PairId<>pairId)
        nu |> List.append <| toKeep |> saveAssociations

    let getAssociatedTags pairId=
        let tagIds= loadAssociations() |> List.filter (fun a -> a.PairId=pairId) |> List.map (fun a -> a.TagId) |> Set.ofList
        loadTags() |> List.filter (fun t -> tagIds.Contains t.Id) |> List.map Tag.Deserialize

    let deserializePair = WordPair.Deserialize getAssociatedTags

    interface IPersistence with 
        member this.UpdateConfiguration config=  config.Serialize() |> saveConfig 
        
        member this.GetConfiguration() =  loadConfig() |> LanguageConfiguration.Deserialize

        member this.AddPair pair = 
            let result= createPair pair
            let tagIds= getTagIds pair.Tags
            updateAssociations result.Id tagIds
            result |> deserializePair

        member this.UpdatePair newPair=
            editPair newPair
            let tagIds= getTagIds newPair.Tags
            updateAssociations (newPair.Id.Serialize()) tagIds
  
        member this.GetPairs() = loadWords() |> List.map deserializePair

        member this.GetTags() = loadTags() |> List.map Tag.Deserialize
        
        member this.UpdateTag newTag = 
            let serialized= newTag.Serialize()
            let others= loadTags() |> List.filter (fun t -> t.Id<>serialized.Id)
            let updated= serialized :: others
            updated |> saveTags

        
