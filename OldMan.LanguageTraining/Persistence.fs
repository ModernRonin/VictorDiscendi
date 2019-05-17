module OldMan.LanguageTraining.Persistence

open System
open Domain

type private Id= uint64

type private PersistentConfiguration = LanguageConfiguration


type private PersistentPair= 
    {
        Id: Id
        Left: string
        Right: string
        Created: DateTime
    }

type private PersistentTag= 
    {
        Id: Id
        Tag: string
    }

type private PersistentTagWordAssociation=
    {
        TagId: Id
        PairId: Id
    }

type private QuestionResult=
    | Correct
    | Incorrect

type private PersistentHistoryRecord=
    {
        PairId: Id
        Timestamp: DateTime
        Result: QuestionResult
        ScoreDelta: int
        Type: QuestionType
        Direction: Direction
    }

type private PersistentScoreCard=
    {
        PairId: Id
        ScoreCard: ScoreCard
    }


type private Database=
    {
        Configuration: PersistentConfiguration
        WordPairs: PersistentPair list
        Tags: PersistentTag list
        TagWordAssociations: PersistentTagWordAssociation list
        ScoreCards: PersistentScoreCard list
        History: PersistentHistoryRecord list
    }



