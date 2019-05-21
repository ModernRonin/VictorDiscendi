module OldMan.LanguageTraining.Service

open System
open OldMan.LanguageTraining.Domain
open OldMan.LanguageTraining.Persistence

type InitialWordPair= 
    {
        Pair: Word*Word
        Tags: Tag list
    }
type WordPairUpdate=
    {
        Old: InitialWordPair
        New: InitialWordPair
    }


type IService=
    abstract member addWordPair: InitialWordPair -> unit
    abstract member updateWordPair: WordPairUpdate -> unit
    abstract member listWordPairs: unit -> WordPair list
    abstract member listWordPairsForTags: TagCondition -> WordPair list
    abstract member listTags: unit -> Tag list
    abstract member updateLanguageNames: LanguageConfiguration -> unit
    abstract member getLanguageNames: unit -> LanguageConfiguration
    abstract member generateQuestion: QuizSettings -> Question
    abstract member scoreQuestionResult: Question -> QuestionResult -> unit
