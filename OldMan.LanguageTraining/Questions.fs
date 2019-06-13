namespace OldMan.LanguageTraining.Domain

type FreeEntryQuestion=
    {
        Prompt: string
        CorrectAnswer: string
    }
module FreeEntryQuestion=
    let from raw= {
                    Prompt= Word.unwrap raw.Question
                    CorrectAnswer= Word.unwrap raw.Answer
                  }


type MultipleChoiceQuestion=
    {
        Prompt: string
        Choices: string list
        CorrectAnswer: string
    }
module MultipleChoiceQuestion=
    let from raw alternativeAnswers= {
                                        Prompt= Word.unwrap raw.Question
                                        CorrectAnswer= Word.unwrap raw.Answer
                                        Choices= Word.unwrap raw.Answer :: alternativeAnswers
                                     }


type Question=
    | FreeEntryQuestion of FreeEntryQuestion
    | MultipleChoiceQuestion of MultipleChoiceQuestion

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
        let question= 
                match settings.Type with
                | FreeEntry 
                    -> FreeEntryQuestion (FreeEntryQuestion.from raw)
                | MultipleChoice choiceSettings 
                    -> MultipleChoiceQuestion 
                        (
                            candidates |> List.except [raw] 
                            |> List.sortBy (fun r -> r.Score) |> List.take (ChoiceCount.minusOne choiceSettings.NumberOfChoices)
                            |> List.map (fun r -> Word.unwrap r.Answer) |> MultipleChoiceQuestion.from raw 
                        )
        RawQuestion.toReference raw, question 
