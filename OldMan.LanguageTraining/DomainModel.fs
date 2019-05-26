module OldMan.LanguageTraining.Domain

open System

// core entities
type Word = Word of string 
module Word=
    let unwrap (Word text)= text

type Score = Score of int 
module Score=
    let start= Score (-3)

type Count = Count of uint32
module Count=
    let zero= Count (uint32 0)
    let unwrap (Count c)= c

type SmallCount= SmallCount of uint8 
module SmallCount=
    let zero= SmallCount (uint8 0)
    let unwrap (SmallCount c)= c
    let minusOne (SmallCount c)= SmallCount (c-uint8 1)

type Id = Id of int64 
module Id=
    let uninitialized= Id (int64 0)
    let from (what: int)= Id (int64 what)

type Tag = 
    {
        Id: Id
        Text: string
    }
module Tag=
    let create tag = 
        {
            Id = Id.uninitialized
            Text= tag
        }

type ScoreCard = 
    {
        LastAsked: DateTime
        TimesAsked: Count
        LeftScore: Score
        RightScore: Score
    }
module ScoreCard=
    let create()=
        {
            LastAsked= DateTime.UtcNow
            TimesAsked= Count.zero
            LeftScore= Score.start
            RightScore= Score.start
        }


type WordPair= 
     {
        Id: Id
        Left: Word
        Right: Word
        Created: DateTime
        Tags: Tag list
        ScoreCard: ScoreCard
     }
module WordPair=
    let create definition=
        let (left, right, tags)= definition
        {
            Left= left
            Right= right
            Tags= tags
            Id = Id.uninitialized
            Created= DateTime.UtcNow
            ScoreCard= ScoreCard.create()
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
module TagCondition=
    let from (tag: string)= tag |> Tag.create |> TagIsContained
    let rec isFulfilledBy condition tags = 
        match condition with
        | TagIsContained t -> tags |> List.contains t
        | AndTagCondition (left, right) -> (isFulfilledBy left tags) && (isFulfilledBy right tags)
        | OrTagCondition (left, right) -> (isFulfilledBy left tags) || (isFulfilledBy right tags)
    let filter condition pairs= pairs |> List.filter (fun p -> isFulfilledBy condition p.Tags)
        

type QuizSettings=
    {
        Direction: Direction
        Type: QuestionType
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
        Prompt: string
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

type RawQuestion=
    {
        PairId: Id
        Question: Word 
        Answer: Word 
        SideOfAnswer: Side
        Score: Score
        LastAsked: DateTime

    }

let toRawQuestions pair=
    [
        {
            PairId= pair.Id
            Question= pair.Left
            Answer= pair.Right
            SideOfAnswer= Right
            Score= pair.ScoreCard.LeftScore
            LastAsked= pair.ScoreCard.LastAsked
        }
        {
            PairId= pair.Id
            Question= pair.Right
            Answer= pair.Left
            SideOfAnswer= Left
            Score= pair.ScoreCard.LeftScore
            LastAsked= pair.ScoreCard.LastAsked
        }
    ]
    

let getCandidates settings pairs= 
    let matchTags= TagCondition.filter settings.TagsToInclude
    let matchSide raw= 
        match settings.Direction with
        | Both -> true
        | LeftToRight -> raw.SideOfAnswer=Right
        | RightToLeft -> raw.SideOfAnswer=Left
    let matchScore raw= raw.Score<=settings.MaximumScore

    pairs |> matchTags |> List.collect toRawQuestions 
          |> List.filter matchSide |> List.filter matchScore


let createQuestion settings pairs=
    let candidates= getCandidates settings pairs 
    let raw = candidates |> List.minBy (fun r -> r.LastAsked)
    match settings.Type with
    | FreeEntry -> FreeEntryQuestion {
                        Prompt= Word.unwrap raw.Question
                        CorrectAnswer= Word.unwrap raw.Answer
                   }
    | MultipleChoice choiceSettings -> 
        MultipleChoiceQuestion {
            Prompt= Word.unwrap raw.Question
            CorrectAnswer= Word.unwrap raw.Answer
            Choices= []//candidates |> List.except [raw] 
                       // |> List.sortBy (fun r -> r.Score) |> List.take choiceSettings.NumberOfChoices-1
                       // |> List.Cons raw |> List.map unwrap 
        }

    
