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

type WordPair= 
     {
        Id: int64
        Pair: Word*Word
        Created: DateTime
        Tags: Tag list
        ScoreCard: ScoreCard
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

type MultipleChoice=
    {
        NumberOfChoices: uint8
    }

type QuestionType=
    | MultipleChoice of MultipleChoice
    | FreeEntry
    

type QuizSettings=
    {
        Direction: Direction
        Types: QuestionType list
        TagsToInclude: Tag list
        MaximumScore: int
    }


type Question= WordPair 
    

type QuestionResult =
    | Correct
    | Incorrect


// querying
type TagCondition=
    | AndTagCondition of TagCondition*TagCondition
    | OrTagCondition of TagCondition*TagCondition
    | TagIsContained of Tag


let rec matches condition tags = 
    match condition with
    | TagIsContained t -> tags |> List.contains t
    | AndTagCondition (left, right) -> (matches left tags) && (matches right tags)
    | OrTagCondition (left, right) -> (matches left tags) || (matches right tags)

                   
