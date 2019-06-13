namespace OldMan.LanguageTraining.Domain

open System

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
