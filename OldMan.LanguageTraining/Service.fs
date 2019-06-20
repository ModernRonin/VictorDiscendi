namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain
open OldMan.LanguageTraining.Persistence


type IService=
    abstract member listWordPairs: (unit) -> WordPair list
    abstract member addWordPair: (Word*Word*Tag list) -> WordPair
    abstract member updateWordPair: Id * (Word*Word*Tag list) -> unit
    abstract member listWordPairsForTags: (TagCondition) -> WordPair list
    abstract member listTags: (unit) -> (Tag*int) list
    abstract member updateLanguageNames: (LanguageConfiguration) -> unit
    abstract member getLanguageNames: (unit) -> LanguageConfiguration
    abstract member generateQuestion: (QuizSettings) -> WordReference*Question
    abstract member scoreQuestionResult: (WordReference*Question*QuestionResult) -> unit

type Service(persistence: IPersistence)=
    interface IService with
        member this.updateLanguageNames(configuration)= persistence.UpdateConfiguration(configuration)
        member this.getLanguageNames()= persistence.GetConfiguration()
        member this.listWordPairs()=  persistence.GetPairs()
        member this.listWordPairsForTags condition= 
            persistence.GetPairs() |> TagCondition.filter condition
        member this.addWordPair(definition)=
            definition |> WordPair.create |> persistence.AddPair 
        member this.updateWordPair (id,definition)=
            {(definition |> WordPair.create) with Id=id} |> persistence.UpdatePair
        member this.listTags()= 
            let pairs= persistence.GetPairs()
            let count tag= pairs |> List.filter (fun p -> p.Tags |> List.contains tag) |> List.length
            pairs |> List.collect (fun p -> p.Tags) |> List.distinct |> List.map (fun t -> (t, (count t)))

        member this.generateQuestion(settings)= persistence.GetPairs() |> Question.create <| settings

        member this.scoreQuestionResult (result: WordReference*Question*QuestionResult)=
            let allPairs= persistence.GetPairs()
            let findPair id= allPairs |> List.find (fun p -> p.Id=id)
            let changed= result |> Scoring.score findPair
            changed |> List.iter persistence.UpdatePair
        


        






