namespace OldMan.LanguageTraining.Domain

type MultipleChoiceSettings=
    {
        NumberOfChoices: ChoiceCount
    }

type QuestionType=
    | MultipleChoice of MultipleChoiceSettings
    | FreeEntry
    

type QuizSettings=
    {
        Direction: Direction
        Type: QuestionType
        TagsToInclude: TagCondition
        MaximumScore: Score
    }


