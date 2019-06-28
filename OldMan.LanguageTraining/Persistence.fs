namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

module Serialization=
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
    

