module OldMan.LanguageTraining.Persistence

open FSharp.Data
open OldMan.LanguageTraining
open OldMan.LanguageTraining.Domain

// CSV Types:
type private CsvConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >
module private CsvConfiguration=
    let toSerializable (csv: CsvConfiguration.Row)= 
        {
            LeftName= csv.LeftLanguageName
            RightName= csv.RightLanguageName
        }
    let fromSerializable (ser: SerializableConfiguration)= 
        CsvConfiguration.Row(ser.LeftName, ser.RightName)

type private CsvPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (string), LastAsked (string), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >
module private CsvPair=
    let toSerializable (csv: CsvPair.Row)=
        {
            Id= csv.Id
            Left= csv.Left
            Right= csv.Right
            Created= csv.Created
            LastAsked= csv.LastAsked
            TimesAsked= csv.TimesAsked
            LeftScore= csv.LeftScore
            RightScore= csv.RightScore
        }
    let fromSerializable (ser: SerializablePair)=
        CsvPair.Row(ser.Id, ser.Left, ser.Right, ser.Created, ser.LastAsked, ser.TimesAsked, ser.LeftScore, ser.RightScore)

type private CsvTag= 
    CsvProvider<
        Schema = "Id (int64), Tag (string)",
        HasHeaders=false
        >
module private CsvTag=
    let toSerializable (csv: CsvTag.Row): SerializableTag=
        {
            Id= csv.Id
            Text= csv.Tag
        }
    let fromSerializable (ser: SerializableTag)=
        CsvTag.Row(ser.Id, ser.Text)


type private CsvTagPairAssociation=
    CsvProvider<
        Schema = "TagId (int64), PairId (int64)",
        HasHeaders=false
        >

module private CsvTagPairAssociation=
    let toSerializable (csv: CsvTagPairAssociation.Row)=
        {
            TagId= csv.TagId
            PairId= csv.PairId
        }
    let fromSerializable (ser: SerializableTagPairAssociation)=
        CsvTagPairAssociation.Row(ser.TagId, ser.PairId)

// serialization extensions




type DataKind=
    | Configuration
    | Words
    | Tagging
    | WordTagAssociation

type Loader= DataKind -> string
type Saver= DataKind -> string -> unit


type CsvPersistenceStore(loader: Loader, saver: Saver)=
    let safeParse parse str= 
        match str with
        | null 
        | "" -> Array.empty
        | _ -> parse str

    interface IPersistenceStore with
        member this.loadPairs()= Words |> loader |> safeParse CsvPair.ParseRows |> Array.map CsvPair.toSerializable |> List.ofArray
        member this.loadTags()= Tagging |> loader |> safeParse CsvTag.ParseRows |> Array.map CsvTag.toSerializable |> List.ofArray
        member this.loadAssociations()= 
            WordTagAssociation |> loader 
            |> safeParse CsvTagPairAssociation.ParseRows 
            |> Array.map CsvTagPairAssociation.toSerializable |> List.ofArray

        member this.savePairs pairs= 
            let csv= pairs |> List.map CsvPair.fromSerializable
            (new CsvPair(csv)).SaveToString() |> saver Words 

        member this.saveTags tags= 
            let csv= tags |> List.map CsvTag.fromSerializable
            (new CsvTag(csv)).SaveToString() |> saver Tagging

        member this.saveAssociations associations= 
            let csv= associations |> List.map CsvTagPairAssociation.fromSerializable
            (new CsvTagPairAssociation(csv)).SaveToString() |> saver WordTagAssociation

        member this.loadConfig()= 
            match Configuration |> loader |> safeParse CsvConfiguration.ParseRows |> List.ofArray with
            | [] -> CsvConfiguration.Row("", "")
            | head::_ -> head
            |> CsvConfiguration.toSerializable

        member this.saveConfig config= 
            let csv= config |> CsvConfiguration.fromSerializable |> Seq.singleton 
            (new CsvConfiguration(csv)).SaveToString() |> saver Configuration
    
open Serialization

type CsvPersistence(loader: Loader, saver: Saver)=
    let store= new CsvPersistenceStore(loader, saver) :> IPersistenceStore


    let createPair (pair: WordPair)= 
        let existing= store.loadPairs()
        let nextId=  Id.nextAfter existing
        let result= {pair with Id=nextId}.Serialize()
        let updated= result |> List.singleton |> List.append existing
        store.savePairs updated
        result

    let editPair (nu: WordPair)=
        let newRecord= nu.Serialize()
        let existingWithoutOld= store.loadPairs() |> List.filter (fun p -> p.Id<>nu.Id.Serialize())
        newRecord :: existingWithoutOld |> store.savePairs
        
    let getTagIds (tags: Tag list)=
        let existing= store.loadTags() 
        let doesNotExistYet (t:Tag) = existing |> List.exists (fun e -> e.Id=t.Id.Serialize()) |> not
        let (newTags, keptTags)= tags |> List.partition doesNotExistYet
        let nextId= Id.nextAfter existing
        let assignNextId (index, tag): Tag = {tag with Id=nextId.AddDelta index}
        let newTagsWithIds= newTags |> List.indexed |> List.map assignNextId
        let removed= keptTags |> List.map (fun t -> t.Serialize()) |> List.except <| existing
        let updated= newTagsWithIds |> List.map (fun t -> t.Serialize()) |> List.append existing
        store.saveTags updated
        updated  |> List.except removed |> List.map Id.fromRaw |> List.map Id.unwrap 

    let removeTags tagIds= 
        let existing= store.loadTags()
        let shouldBeKept (t: SerializableTag) = tagIds |> List.contains t.Id |> not
        let updated= existing |> List.filter shouldBeKept
        updated |> store.saveTags

    let updateAssociations pairId tagIds=
        let nu= tagIds |> List.map (fun tagId -> serializePairTagAssociation pairId tagId)
        let old= store.loadAssociations()
        let (oldForPair, toKeep) = old |> List.partition (fun a -> a.PairId=pairId)
        nu |> List.append <| toKeep |> store.saveAssociations
        let deletedIds= oldForPair |> List.map (fun a -> a.TagId) |> List.except tagIds
        let isNotInUse tagId= toKeep |> List.map (fun a -> a.TagId) |> List.contains tagId |> not
        deletedIds |>  List.filter isNotInUse |> removeTags

    let getAssociatedTags pairId=
        let tagIds= store.loadAssociations() |> List.filter (fun a -> a.PairId=pairId) |> List.map (fun a -> a.TagId) |> Set.ofList
        store.loadTags() |> List.filter (fun t -> tagIds.Contains t.Id) |> List.map Tag.Deserialize

    let deserializePair = WordPair.Deserialize getAssociatedTags

    interface IPersistence with 
        member this.UpdateConfiguration config=  config.Serialize() |> store.saveConfig 
        
        member this.GetConfiguration() =  store.loadConfig() |> LanguageConfiguration.Deserialize

        member this.AddPair pair = 
            let result= createPair pair
            let tagIds= getTagIds pair.Tags
            updateAssociations result.Id tagIds
            result |> deserializePair

        member this.UpdatePair newPair=
            editPair newPair
            let tagIds= getTagIds newPair.Tags
            updateAssociations (newPair.Id.Serialize()) tagIds
  
        member this.GetPairs() = store.loadPairs() |> List.map deserializePair

        member this.GetTags() = store.loadTags() |> List.map Tag.Deserialize |> List.distinct
        
        member this.AddOrUpdateTag newTag = 
            let add()=
                let existing= store.loadTags() 
                let id= Id.nextAfter existing
                let result= {newTag with Id= id}
                result.Serialize() :: existing |> store.saveTags
                result
            let update()= 
                let serialized= newTag.Serialize()
                let others= store.loadTags() |> List.filter (fun t -> t.Id<>serialized.Id)
                serialized :: others |> store.saveTags
                newTag
            
            if newTag.Id=Id.uninitialized then add() else update()

        
