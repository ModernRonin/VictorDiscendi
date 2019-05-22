module OldMan.LanguageTraining.Domain

open System

// core entities
type Word = string option

type Tag = string

type ScoreCard = 
    {
        LastAsked: DateTime
        TimesAsked: uint32
        LeftScore: int
        RightScore: int
    }

let createNewScoreCard()=
    {
        LastAsked= DateTime.UtcNow
        TimesAsked= uint32 0
        LeftScore= -3
        RightScore= -3
    }


type WordPair= 
     {
        Id: int64
        Pair: Word*Word
        Created: DateTime
        Tags: Tag list
        ScoreCard: ScoreCard
     }

let createNewWordPair words tags=
    {
        Pair = words
        Tags= tags
        Id = 0
        Created= DateTime.UtcNow
        ScoreCard= createNewScoreCard()
    }


type LanguageConfiguration= 
    {
        LeftLanguageName: string
        RightLanguageName: string
    }

// quizzes
type Direction=
    | LeftToRight
    | RightToLeft
    | Both

type MultipleChoiceSettings=
    {
        NumberOfChoices: uint8
    }

type QuestionType=
    | MultipleChoice of MultipleChoiceSettings
    | FreeEntry
    

type QuizSettings=
    {
        Direction: Direction
        Types: QuestionType list
        TagsToInclude: Tag list
        MaximumScore: int
    }

type Side= 
    | Left
    | Right

type FreeEntryQuestion=
    {
        Prompt: string
        CorrectAnswer: string
    }

type MultipleChoiceQuestion=
    {
        Choices: string list
        CorrectAnswer: string
    }

type Question=
    | FreeEntryQuestion of FreeEntryQuestion
    | MultipleChoiceQuestion of MultipleChoiceQuestion

type WordReference= 
    {
        PairId: int64
        Side: Side
    }
    

type QuestionResult =
    | Correct
    | Incorrect


// querying
type TagCondition=
    | AndTagCondition of TagCondition*TagCondition
    | OrTagCondition of TagCondition*TagCondition
    | TagIsContained of Tag


let rec doTagsMatch condition tags = 
    match condition with
    | TagIsContained t -> tags |> List.contains t
    | AndTagCondition (left, right) -> (doTagsMatch left tags) && (doTagsMatch right tags)
    | OrTagCondition (left, right) -> (doTagsMatch left tags) || (doTagsMatch right tags)

                   
