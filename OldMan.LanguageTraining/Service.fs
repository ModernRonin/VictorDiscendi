module OldMan.LanguageTraining.Service

open OldMan.LanguageTraining.Domain
open OldMan.LanguageTraining.Persistence


type IService=
    abstract member addWordPair: Word*Word*Tag list -> unit
    abstract member updateWordPair: int64 -> Word*Word*Tag list -> unit
    abstract member listWordPairs: unit -> WordPair list
    abstract member listWordPairsForTags: TagCondition -> WordPair list
    abstract member listTags: unit -> Tag list
    abstract member updateLanguageNames: LanguageConfiguration -> unit
    abstract member getLanguageNames: unit -> LanguageConfiguration
    abstract member generateQuestion: QuizSettings -> Question
    abstract member scoreQuestionResult: Question -> QuestionResult -> unit
