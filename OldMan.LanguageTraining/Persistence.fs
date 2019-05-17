module OldMan.LanguageTraining.Persistence

open System
open FSharp.Data
open Domain

// persistence types
type PersistentConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >

type private PersistentPair= 
    CsvProvider<
        Schema = "Id (uint64), Left (string), Right (string), Created (DateTime)",
        HasHeaders=false
        >

type private PersistentTag= 
    CsvProvider<
        Schema = "Id (uint64), Tag (string)",
        HasHeaders=false
        >

type private PersistentTagWordAssociation=
    CsvProvider<
        Schema = "TagId (uint64), PairId (uint64)",
        HasHeaders=false
        >

type private PersistentHistoryRecord=
    CsvProvider<
        Schema = "PairId (uint64), Timestamp (DateTime), WasCorrect (bool), ScoreDelta (int), Type (int), Direction (int)",
        HasHeaders=false
        >

type private PersistentScoreCard=
    CsvProvider<
        Schema = "PairId (uint64), LastAsked (DateTime), TimesAsked (uint32), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >

type private Database=
    {
        Configuration: PersistentConfiguration
        WordPairs: PersistentPair list
        Tags: PersistentTag list
        TagWordAssociations: PersistentTagWordAssociation list
        ScoreCards: PersistentScoreCard list
        History: PersistentHistoryRecord list
    }

open System.IO

let private saveConfiguration (path: string) config=
    let csv= new PersistentConfiguration(
            [
                PersistentConfiguration.Row(config.LeftLanguageName, config.RightLanguageName)
            ]
        )
    csv.Save path

let private loadConfiguration (path: string) config=
    let csv=  PersistentConfiguration.Load path
    let single= csv.Rows |> Seq.head
    {LeftLanguageName= single.LeftLanguageName; RightLanguageName= single.RightLanguageName}

type Persistence(directory: string)=
    let configPath= Path.Combine(directory, "LanguageConfiguration")
    member this.SaveConfiguration =  saveConfiguration configPath
    member this.LoadConfiguration (config: LanguageConfiguration)=  loadConfiguration configPath


        
        


