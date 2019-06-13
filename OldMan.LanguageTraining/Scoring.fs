namespace OldMan.LanguageTraining.Domain

type QuestionResult =
    | Correct
    | Incorrect

module Scoring= 
    let scorePair pair side delta=
        match side with
        | Left -> { pair with ScoreCard= {pair.ScoreCard with LeftScore=Score.add pair.ScoreCard.LeftScore delta} }
        | Right -> { pair with ScoreCard= {pair.ScoreCard with RightScore=Score.add pair.ScoreCard.RightScore delta} }

    let score (pairFinder: Id->WordPair) (result: WordReference*Question*QuestionResult) : WordPair list= 
        let (reference, question, questionResult)= result
        let pair= pairFinder reference.PairId
        let scorePair= scorePair pair reference.Side
        match (question, questionResult) with
            | (FreeEntryQuestion _, Incorrect) -> -1 |> scorePair |> List.singleton
            | (FreeEntryQuestion _, Correct) -> 1 |> scorePair  |> List.singleton
            | (MultipleChoiceQuestion choices, Correct) -> choices.Choices.Length |> scorePair |> List.singleton
            | (MultipleChoiceQuestion choices, Incorrect) -> -choices.Choices.Length |> scorePair |> List.singleton

