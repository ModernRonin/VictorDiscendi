namespace OldMan.LanguageTraining.Domain

open System
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



type RawQuestion=
    {
        PairId: Id
        Question: Word 
        Answer: Word 
        SideOfAnswer: Side
        Score: Score
        LastAsked: DateTime
    }
module RawQuestion=
    let toFreeEntry raw= FreeEntryQuestion 
                            {
                                Prompt= Word.unwrap raw.Question
                                CorrectAnswer= Word.unwrap raw.Answer
                            }
    let toMultipleChoice raw alternativeAnswers= MultipleChoiceQuestion 
                                                    {
                                                        Prompt= Word.unwrap raw.Question
                                                        CorrectAnswer= Word.unwrap raw.Answer
                                                        Choices= Word.unwrap raw.Answer :: alternativeAnswers
                                                    }
    let ofPair pair= 
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
    let toReference raw= 
        {
            PairId= raw.PairId
            Side= raw.SideOfAnswer
        }

module Question=
    let private getCandidates settings pairs= 
        let matchTags= TagCondition.filter settings.TagsToInclude
        let matchSide raw= 
            match settings.Direction with
            | Both -> true
            | LeftToRight -> raw.SideOfAnswer=Right
            | RightToLeft -> raw.SideOfAnswer=Left
        let matchScore raw= raw.Score<=settings.MaximumScore

        pairs |> matchTags |> List.collect RawQuestion.ofPair 
              |> List.filter matchSide |> List.filter matchScore

    let create pairs settings=
        let candidates= getCandidates settings pairs 
        let raw = candidates |> List.minBy (fun r -> r.LastAsked)
        let question= match settings.Type with
                        | FreeEntry -> RawQuestion.toFreeEntry raw
                        | MultipleChoice choiceSettings -> 
                                candidates |> List.except [raw] 
                                |> List.sortBy (fun r -> r.Score) |> List.take (ChoiceCount.minusOne choiceSettings.NumberOfChoices)
                                |> List.map (fun r -> Word.unwrap r.Answer) |> RawQuestion.toMultipleChoice raw 
        RawQuestion.toReference raw, question 
