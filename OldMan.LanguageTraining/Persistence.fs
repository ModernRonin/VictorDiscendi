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

    let timeFormat= "yyyy-MM-dd-HH-mm-ss"
    type DateTime with
        member this.Serialize()= 
            match WebSharper.Pervasives.IsClient with
            | false -> this.ToString(timeFormat)
            | true -> 
                // WebSharper does not know the ToString() overload which is passed a format
                sprintf "%04i-%02i-%02i-%02i-%02i-%02i" this.Year this.Month this.Day this.Hour this.Minute this.Second

        static member Deserialize (from: string)= 
            match WebSharper.Pervasives.IsClient with
            | false -> DateTime.ParseExact(from, timeFormat, CultureInfo.InvariantCulture)
            | true ->
                // can't use DateTime.ParseExact() because WebSharper does not know CultureInfo
                match from.Split('-') |> Array.map (fun s -> Int32.Parse s) |> List.ofArray with
                | [year; month; day; hour; minute; second] -> new DateTime(year, month, day, hour, minute, second)
                | _ -> failwith "Invalid datetime format"
    
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
            async {
                let! tags= tagsLoader pair.Id
                return {
                    Id = pair.Id |> Id.Deserialize
                    Left= pair.Left |> Word.Deserialize
                    Right= pair.Right |> Word.Deserialize
                    Created = pair.Created |> DateTime.Deserialize
                    Tags = tags
                    ScoreCard= 
                        {
                            LastAsked=  pair.LastAsked |> DateTime.Deserialize
                            TimesAsked= pair.TimesAsked |> Count.Deserialize
                            LeftScore= pair.LeftScore |> Score.Deserialize
                            RightScore= pair.RightScore |> Score.Deserialize
                        }
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
            async {
                let!existing= store.loadPairs()
                let nextId=  Id.nextAfter existing
                let result= {pair with Id=nextId}.Serialize()
                let updated= result |> List.singleton |> List.append existing
                do! store.savePairs updated
                return result
            }
    
        let editPairs (nus: WordPair list)=
            async {
                let newRecords= nus |> List.map (fun p -> p.Serialize())
                let! existing= store.loadPairs()
                let isIdOfUpdated id = newRecords |> List.exists (fun r -> r.Id=id)
                let existingWithoutOld= existing |> List.filter (fun p -> p.Id |> isIdOfUpdated |> not )
                let newAll= List.append newRecords existingWithoutOld
                do! newAll |> store.savePairs
            }

        let getTagIds (tags: Tag list)=
            async {
                let! existing= store.loadTags() 
                let doesNotExistYet (t:Tag) = existing |> List.exists (fun e -> e.Id=t.Id.Serialize()) |> not
                let (newTags, keptTags)= tags |> List.partition doesNotExistYet
                let nextId= Id.nextAfter existing
                let assignNextId (index, tag): Tag = {tag with Id=nextId.AddDelta index}
                let newTagsWithIds= newTags |> List.indexed |> List.map assignNextId
                let removed= keptTags |> List.map (fun t -> t.Serialize()) |> List.except <| existing
                let updated= newTagsWithIds |> List.map (fun t -> t.Serialize()) |> List.append existing
                do! store.saveTags updated
                return updated  |> List.except removed |> List.map Id.fromRaw |> List.map Id.unwrap 
            }    
        let removeTags tagIds= 
            async {
                let! existing= store.loadTags()
                let shouldBeKept (t: SerializableTag) = tagIds |> List.contains t.Id |> not
                let updated= existing |> List.filter shouldBeKept
                do! updated |> store.saveTags
            }
    
        let updateAssociations pairId tagIds=
            async {
                let nu= tagIds |> List.map (fun tagId -> serializePairTagAssociation pairId tagId)
                let! old= store.loadAssociations()
                let (oldForPair, toKeep) = old |> List.partition (fun a -> a.PairId=pairId)
                do! nu |> List.append <| toKeep |> store.saveAssociations
                let deletedIds= oldForPair |> List.map (fun a -> a.TagId) |> List.except tagIds
                let isNotInUse tagId= toKeep |> List.map (fun a -> a.TagId) |> List.contains tagId |> not
                do! deletedIds |>  List.filter isNotInUse |> removeTags
            }

        let getAssociatedTags pairId=
            async {
                let! associations= store.loadAssociations()
                let tagIds= associations |> List.filter (fun a -> a.PairId=pairId) |> List.map (fun a -> a.TagId) |> Set.ofList
                let! tags= store.loadTags()
                return tags |> List.filter (fun t -> tagIds.Contains t.Id) |> List.map Tag.Deserialize
            }

        let deserializePair = 
            WordPair.Deserialize getAssociatedTags
    
        member this.UpdateConfiguration(config: LanguageConfiguration)= 
            config.Serialize() |> store.saveConfig 
            
        member this.GetConfiguration()=  
            async {
                let! cfg= store.loadConfig()
                return cfg |> LanguageConfiguration.Deserialize
            }
    
        member this.AddPair pair = 
            async{
                let! result= createPair pair
                let! tagIds= getTagIds pair.Tags
                do! updateAssociations result.Id tagIds
                return! (result |> deserializePair)
            }
    
        member this.UpdatePair newPair=
            async {
                do! newPair |> List.singleton |> editPairs
                let! tagIds= getTagIds newPair.Tags
                do! updateAssociations (newPair.Id.Serialize()) tagIds
            }
      
        member this.UpdateScores(pairs: WordPair list)= 
            pairs |> editPairs

        member this.GetPairs() = 
            async {
                let! pairs= store.loadPairs() 
                let! result= pairs |> List.map deserializePair |> Async.Parallel
                return result |> List.ofArray
            }
    
        member this.GetTags() = 
            async {
                let! tags= store.loadTags() 
                return tags |> List.map Tag.Deserialize |> List.distinct
            }
        member this.AddOrUpdateTag (newTag: Tag) = 
            let add()=
                async {
                    let! existing= store.loadTags() 
                    let id= Id.nextAfter existing
                    let result= {newTag with Id= id}
                    do! result.Serialize() :: existing |> store.saveTags
                    return result
                }
            let update()= 
                async {
                    let serialized= newTag.Serialize()
                    let! tags= store.loadTags()
                    let others= tags |> List.filter (fun t -> t.Id<>serialized.Id)
                    do! serialized :: others |> store.saveTags
                    return newTag
                }
                
            if newTag.Id=Id.uninitialized then add() else update()
    
            
    
