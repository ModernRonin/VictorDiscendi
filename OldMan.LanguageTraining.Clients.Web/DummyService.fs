module OldMan.LanguageTraining.Dummy

open System
open OldMan.LanguageTraining
open OldMan.LanguageTraining.Domain

type Service()=
    let pair english german tags= 
        WordPair.create (Word english, Word german, tags |> List.map Tag.create)
    let mutable pairs= 
        [
            pair "apple" "Apfel" ["noun"; "fruit"]
            pair "orange" "Orange" ["noun"; "fruit"]
            pair "peel" "schälen" ["verb"; "cooking"]
            pair "red" "rot" ["adjective"; "color"]
            {
                pair "old" "alt" ["adjective"] 
                with 
                    Created= new DateTime(1974, 7, 23, 0, 2, 0, DateTimeKind.Utc)
                    ScoreCard= 
                    {
                        LastAsked= new DateTime(2018, 02, 03, 16, 30, 0, DateTimeKind.Utc)
                        TimesAsked= Count 13u
                        LeftScore= Score 7
                        RightScore= Score 5
                    }
            }
        ]
    interface IService with
        member this.updateLanguageNames(configuration)= ()

        member this.getLanguageNames()= {LeftLanguageName="English"; RightLanguageName="German"}

        member this.listWordPairs()= pairs

        member this.listWordPairsForTags condition= 
            pairs |> TagCondition.filter condition

        member this.addWordPair(definition)=
            let result= definition |> WordPair.create
            pairs <- result :: pairs
            result

        member this.updateWordPair (id,definition)=
            let withoutOld= pairs |> List.filter (fun p -> p.Id<>id)
            let updated= {(definition |> WordPair.create) with Id=id}
            pairs <- updated :: withoutOld

        member this.listTags()= 
            let count tag= pairs |> List.filter (fun p -> p.Tags |> List.contains tag) |> List.length
            pairs |> List.collect(fun p -> p.Tags) |> List.distinct |> List.map (fun t -> (t, (count t)))

        member this.generateQuestion(settings)= 
            match settings.Type with
                | FreeEntry -> 
                    (
                        {PairId = Id.wrap 1L; Side=Right}, 
                        FreeEntryQuestion { Prompt= "Apple"; CorrectAnswer="Apfel"}
                    )
                | MultipleChoice _ -> 
                    (
                        {PairId = Id.wrap 2L; Side=Right}, 
                        MultipleChoiceQuestion {
                                                  Prompt="Orange"
                                                  CorrectAnswer= "orange"
                                                  Choices= ["orange"; "apple"; "peel"]
                                               }
                    )                    

        member this.scoreQuestionResult (({PairId= pairId}, _, result) :WordReference*Question*QuestionResult)=
            let target= pairs |> List.find (fun p -> p.Id=pairId)
            let delta= match result with 
                        | Correct -> 1
                        | Incorrect -> -1
            let withoutOld= pairs |> List.filter (fun p -> p.Id<>pairId)
            let updated= {target with ScoreCard= {target.ScoreCard with RightScore=Score.add target.ScoreCard.RightScore delta}}
            pairs <- updated :: withoutOld
    

