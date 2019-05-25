﻿module OldMan.LanguageTraining.Service

open OldMan.LanguageTraining.Domain
open OldMan.LanguageTraining.Persistence


type IService=
    abstract member addWordPair: Word*Word*Tag list -> WordPair
    abstract member updateWordPair: int64 -> Word*Word*Tag list -> unit
    abstract member listWordPairs: unit -> WordPair list
    abstract member listWordPairsForTags: TagCondition -> WordPair list
    abstract member listTags: unit -> Tag list
    abstract member updateLanguageNames: LanguageConfiguration -> unit
    abstract member getLanguageNames: unit -> LanguageConfiguration
    abstract member generateQuestion: QuizSettings -> WordReference*Question
    abstract member scoreQuestionResult: WordReference*Question*QuestionResult -> unit


type Service(persistence: IPersistence)=
    let toWordPair (definition: Word*Word*Tag list)=
        let (left, right, tags)= definition
        createNewWordPair (left, right) tags 

    //interface IService with
    member this.listWordPairs=  persistence.GetPairs
    member this.updateLanguageNames= persistence.UpdateConfiguration
    member this.getLanguageNames= persistence.GetConfiguration

    member this.addWordPair (definition: Word*Word*Tag list)=
        definition |> toWordPair |> persistence.AddPair 

    member this.updateWordPair id (definition: Word*Word*Tag list)=
        {(definition |> toWordPair) with Id=id} |> persistence.UpdatePair


    member this.listWordPairsForTags condition=
        this.listWordPairs() |> matchTags condition

    member this.listTags()= persistence.GetTags()

    //member this.generateQuestion= createQuestion

    //member this.scoreQuestionResult= scoreQuestion
        
        






