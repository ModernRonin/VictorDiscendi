namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

type IService=
    abstract member ListWordPairs: (unit) -> WordPair list
    abstract member AddWordPair: (Word*Word*Tag list) -> WordPair
    abstract member UpdateWordPair: Id * (Word*Word*Tag list) -> unit
    abstract member ListWordPairsForTags: (TagCondition) -> WordPair list
    abstract member ListTags: (unit) -> (Tag*int) list
    abstract member AddOrUpdateTag: (Tag) -> Tag
    abstract member UpdateLanguageNames: (LanguageConfiguration) -> unit
    abstract member GetLanguageNames: (unit) -> LanguageConfiguration
    abstract member GenerateQuestion: (QuizSettings) -> WordReference*Question
    abstract member ScoreQuestionResult: (WordReference*Question*QuestionResult) -> unit

type Service(store: IPersistenceStore)=
    let persistence= new Persistence(store)

    interface IService with
        member this.UpdateLanguageNames(configuration)= persistence.UpdateConfiguration(configuration)
        member this.GetLanguageNames()= persistence.GetConfiguration()
        member this.ListWordPairs()=  persistence.GetPairs()
        member this.ListWordPairsForTags condition= 
            persistence.GetPairs() |> TagCondition.filter condition
        member this.AddWordPair(definition)=
            definition |> WordPair.create |> persistence.AddPair 
        member this.UpdateWordPair (id,definition)=
            {(definition |> WordPair.create) with Id=id} |> persistence.UpdatePair
        member this.ListTags()= 
            let pairs= persistence.GetPairs()
            let count tag= pairs |> List.filter (fun p -> p.Tags |> List.contains tag) |> List.length
            pairs |> List.collect (fun p -> p.Tags) |> List.distinct |> List.map (fun t -> (t, (count t)))

        member this.AddOrUpdateTag(tag)= persistence.AddOrUpdateTag tag
            
        member this.GenerateQuestion(settings)= persistence.GetPairs() |> Question.create <| settings

        member this.ScoreQuestionResult (result: WordReference*Question*QuestionResult)=
            let allPairs= persistence.GetPairs()
            let findPair id= allPairs |> List.find (fun p -> p.Id=id)
            let changed= result |> Scoring.score findPair
            changed |> List.iter persistence.UpdatePair
        


        






