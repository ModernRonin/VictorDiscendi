namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

type IService=
    abstract member ListWordPairs: (unit) -> Async<WordPair list>
    abstract member AddWordPair: (Word*Word*Tag list) -> Async<WordPair>
    abstract member UpdateWordPair: Id * (Word*Word*Tag list) -> Async<unit>
    abstract member ListWordPairsForTags: (TagCondition) -> Async<WordPair list>
    abstract member ListTags: (unit) -> Async<(Tag*int) list>
    abstract member AddOrUpdateTag: (Tag) -> Async<Tag>
    abstract member UpdateLanguageNames: (LanguageConfiguration) -> Async<unit>
    abstract member GetLanguageNames: (unit) -> Async<LanguageConfiguration>
    abstract member GenerateQuestion: (QuizSettings) -> Async<WordReference*Question>
    abstract member ScoreQuestionResult: (WordReference*Question*QuestionResult) -> Async<unit>

type Service(store: IPersistenceStore)=
    let persistence= new Persistence(store)

    interface IService with
        member this.UpdateLanguageNames(configuration)= persistence.UpdateConfiguration(configuration)
        member this.GetLanguageNames()= persistence.GetConfiguration()
        member this.ListWordPairs()=  persistence.GetPairs()
        member this.ListWordPairsForTags condition= 
            async {
                let! pairs= persistence.GetPairs()
                return pairs |> TagCondition.filter condition
            }
        member this.AddWordPair(definition)=
            definition |> WordPair.create |> persistence.AddPair 
        member this.UpdateWordPair (id,definition)=
            {(definition |> WordPair.create) with Id=id} |> persistence.UpdatePair
        member this.ListTags()= 
            async {
                let! pairs= persistence.GetPairs()
                let count tag= pairs |> List.filter (fun p -> p.Tags |> List.contains tag) |> List.length
                return pairs |> List.collect (fun p -> p.Tags) |> List.distinct |> List.map (fun t -> (t, (count t)))
            }
        member this.AddOrUpdateTag(tag)= persistence.AddOrUpdateTag tag
            
        member this.GenerateQuestion(settings)= 
            async {
                let! pairs= persistence.GetPairs() 
                return pairs |> Question.create <| settings
            }

        member this.ScoreQuestionResult (result: WordReference*Question*QuestionResult)=
            async {
                let! allPairs= persistence.GetPairs()
                let findPair id= allPairs |> List.find (fun p -> p.Id=id)
                let changed= result |> Scoring.score findPair
                
                do! persistence.UpdateScores changed
            }
        


        






