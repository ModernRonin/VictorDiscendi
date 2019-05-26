module OldMan.LanguageTraining.Domain

open System

// core entities
type Word = Word of string 
    

type Score = Score of int with
    static member Start= Score (-3)

type Count = Count of uint32 with
    static member Zero= Count (uint32 0)

type SmallCount= SmallCount of uint8 with
    static member Zero= SmallCount (uint8 0)

type Id = Id of int64 with
    static member Uninitialized= Id (int64 0)
    static member From (what: int)= Id (int64 what)

type Tag = 
    {
        Id: Id
        Text: string
    } with
    static member Create tag = 
        {
            Id = Id.Uninitialized
            Text= tag
        }

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
        Left: Word
        Right: Word
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
    | TagIsContained of Tag with
    static member From (tag: string)= tag |> Tag.Create |> TagIsContained
        

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




// operations
let createNewScoreCard()=
    {
        LastAsked= DateTime.UtcNow
        TimesAsked= Count.Zero
        LeftScore= Score.Start
        RightScore= Score.Start
    }

let createNewWordPair definition=
    let (left, right, tags)= definition
    {
        Left= left
        Right= right
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
    

let matchTags condition pairs= pairs |> List.filter (fun p -> doTagsMatch condition p.Tags)

let getCandidates settings pairs= 
    let matchTags= matchTags settings.TagsToInclude
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
    let unwrap (Word text)= text
    match settings.Type with
    | FreeEntry -> FreeEntryQuestion {
                        Prompt= unwrap raw.Question
                        CorrectAnswer= unwrap raw.Answer
                   }
    | MultipleChoice choiceSettings -> 
        MultipleChoiceQuestion {
            Prompt= unwrap raw.Question
            CorrectAnswer= unwrap raw.Answer
            Choices= []//candidates |> List.except [raw] 
                       // |> List.sortBy (fun r -> r.Score) |> List.take choiceSettings.NumberOfChoices-1
                       // |> List.Cons raw |> List.map unwrap 
        }

    
