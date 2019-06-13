namespace OldMan.LanguageTraining.Domain

open System

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
                Score= pair.ScoreCard.RightScore
                LastAsked= pair.ScoreCard.LastAsked
            }
        ]
    let toReference raw= 
        {
            PairId= raw.PairId
            Side= raw.SideOfAnswer
        }
