namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

module private Serialization=
    open System
    open System.Globalization

    type Count with
        member this.Serialize()= 
            match this with
            | Count x -> int64 x

        static member Deserialize (from: int64)=
            match from with 
            | x when int64 0<=x -> Count(uint32 x)
            | _ -> raise  (ArgumentOutOfRangeException ("count"))

    type Score with 
        member this.Serialize()=
            match this with 
            | Score x -> x
        static member Deserialize (from: int)= Score from

    type Id with 
        member this.Serialize()= 
            match this with 
            | Id x -> x
        static member Deserialize (from: int64)= Id from
        member this.AddDelta (delta: int)= Id.Deserialize (this.Serialize() + int64 delta)

    type Word with
        member this.Serialize()=
            match this with
            | Word x -> x
        static member Deserialize (from: string)= Word from

    let timeFormat= "yyyyMMddHHmmss"
    type DateTime with
        member this.Serialize()= this.ToString(timeFormat, CultureInfo.InvariantCulture)
        static member Deserialize (from: string)= DateTime.ParseExact(from, timeFormat, CultureInfo.InvariantCulture)
    
    type LanguageConfiguration with
        member this.Serialize()= 
            {
                LeftName= this.LeftLanguageName
                RightName= this.RightLanguageName
            }
        static member Deserialize (from: SerializableConfiguration)= 
            {
                LeftLanguageName= from.LeftName
                RightLanguageName= from.RightName
            }

    type WordPair with 
        member this.Serialize() = 
            {
                Id= this.Id.Serialize()
                Left= this.Left.Serialize()
                Right= this.Right.Serialize()
                Created= this.Created.Serialize()
                LastAsked= this.ScoreCard.LastAsked.Serialize()
                TimesAsked= this.ScoreCard.TimesAsked.Serialize()
                LeftScore= this.ScoreCard.LeftScore.Serialize() 
                RightScore= this.ScoreCard.RightScore.Serialize()
            }

        static member Deserialize tagsLoader (pair: SerializablePair) =
            {
                Id = pair.Id |> Id.Deserialize
                Left= pair.Left |> Word.Deserialize
                Right= pair.Right |> Word.Deserialize
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

    type Tag with
        member this.Serialize() : SerializableTag= 
            {
                Id= this.Id.Serialize()
                Text= this.Text
            }

        static member Deserialize (from: SerializableTag) = 
            {
                Id= from.Id |> Id.Deserialize
                Text= from.Text
            }

    let serializePairTagAssociation pairId tagId= 
        {
            TagId= tagId
            PairId= pairId
        }
    
open Serialization
type Persistence(store: IPersistenceStore)=
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
    
        member this.UpdateConfiguration(config: LanguageConfiguration)= 
            config.Serialize() |> store.saveConfig 
            
        member this.GetConfiguration()=  
            store.loadConfig() |> LanguageConfiguration.Deserialize
    
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
            
        member this.AddOrUpdateTag (newTag: Tag) = 
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
    
            
    
