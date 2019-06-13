namespace OldMan.LanguageTraining.Domain

open System

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
