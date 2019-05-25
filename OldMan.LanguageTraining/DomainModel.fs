module OldMan.LanguageTraining.Domain

open System

// core entities
type Word = Word of (string option)

type Tag = string

type Score = Score of int with
    static member Start= Score (-3)

type Count = Count of uint32 with
    static member Zero= Count (uint32 0)

type SmallCount= SmallCount of uint8 with
    static member Zero= SmallCount (uint8 0)

type Id = Id of int64 with
    static member Uninitialized= Id (int64 0)

type ScoreCard = 
    {
        LastAsked: DateTime
        TimesAsked: Count
        LeftScore: Score
        RightScore: Score
    }

type WordPair= 
     {
        Id: Id
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

type MultipleChoiceSettings=
    {
        NumberOfChoices: SmallCount
    }

type QuestionType=
    | MultipleChoice of MultipleChoiceSettings
    | FreeEntry
    
type TagCondition=
    | AndTagCondition of TagCondition*TagCondition
    | OrTagCondition of TagCondition*TagCondition
    | TagIsContained of Tag

type QuizSettings=
    {
        Direction: Direction
        Types: QuestionType list
        TagsToInclude: TagCondition
        MaximumScore: Score
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
        PairId: Id
        Side: Side
    }
    
type QuestionResult =
    | Correct
    | Incorrect




// operations
let createNewScoreCard()=
    {
        LastAsked= DateTime.UtcNow
        TimesAsked= Count.Zero
        LeftScore= Score.Start
        RightScore= Score.Start
    }

let createNewWordPair words tags=
    {
        Pair = words
        Tags= tags
        Id = Id.Uninitialized
        Created= DateTime.UtcNow
        ScoreCard= createNewScoreCard()
    }

let rec doTagsMatch condition tags = 
    match condition with
    | TagIsContained t -> tags |> List.contains t
    | AndTagCondition (left, right) -> (doTagsMatch left tags) && (doTagsMatch right tags)
    | OrTagCondition (left, right) -> (doTagsMatch left tags) || (doTagsMatch right tags)

type RawQuestion=
    {
        PairId: Id

    }


let matchTags condition pairs= pairs |> List.filter (fun p -> doTagsMatch condition p.Tags)

let getScoreForDirection direction pair = 
    match direction with
    | LeftToRight -> pair.ScoreCard.LeftScore
    | RightToLeft -> pair.ScoreCard.RightScore
    | Both -> [pair.ScoreCard.LeftScore; pair.ScoreCard.RightScore] |> List.min

let matchScore direction maxScore pairs = 
    pairs |> List.filter (fun p -> getScoreForDirection direction p <= maxScore)

let getCandidates settings pairs= 
    let matchTags= matchTags settings.TagsToInclude
    let matchScore= matchScore settings.Direction settings.MaximumScore
    pairs |> matchTags |> matchScore
    
