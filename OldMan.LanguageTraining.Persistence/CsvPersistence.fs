module OldMan.LanguageTraining.Persistence

open FSharp.Data
open OldMan
open OldMan.LanguageTraining

// CSV Types:
type private CsvConfiguration = 
    CsvProvider<
        Schema = "LeftLanguageName (string), RightLanguageName (string)",
        HasHeaders=false
        >
module private CsvConfiguration=
    let toSerializable (csv: CsvConfiguration.Row)= 
        {
            LeftName= csv.LeftLanguageName
            RightName= csv.RightLanguageName
        }
    let fromSerializable (ser: SerializableConfiguration)= 
        CsvConfiguration.Row(ser.LeftName, ser.RightName)

type private CsvPair= 
    CsvProvider<
        Schema = "Id (int64), Left (string), Right (string), Created (string), LastAsked (string), TimesAsked (int64), LeftScore (int), RightScore (int)",
        HasHeaders=false
        >
module private CsvPair=
    let toSerializable (csv: CsvPair.Row)=
        {
            Id= csv.Id
            Left= csv.Left
            Right= csv.Right
            Created= csv.Created
            LastAsked= csv.LastAsked
            TimesAsked= csv.TimesAsked
            LeftScore= csv.LeftScore
            RightScore= csv.RightScore
        }
    let fromSerializable (ser: SerializablePair)=
        CsvPair.Row(ser.Id, ser.Left, ser.Right, ser.Created, ser.LastAsked, ser.TimesAsked, ser.LeftScore, ser.RightScore)

type private CsvTag= 
    CsvProvider<
        Schema = "Id (int64), Tag (string)",
        HasHeaders=false
        >
module private CsvTag=
    let toSerializable (csv: CsvTag.Row): SerializableTag=
        {
            Id= csv.Id
            Text= csv.Tag
        }
    let fromSerializable (ser: SerializableTag)=
        CsvTag.Row(ser.Id, ser.Text)


type private CsvTagPairAssociation=
    CsvProvider<
        Schema = "TagId (int64), PairId (int64)",
        HasHeaders=false
        >

module private CsvTagPairAssociation=
    let toSerializable (csv: CsvTagPairAssociation.Row)=
        {
            TagId= csv.TagId
            PairId= csv.PairId
        }
    let fromSerializable (ser: SerializableTagPairAssociation)=
        CsvTagPairAssociation.Row(ser.TagId, ser.PairId)

type DataKind=
    | Configuration
    | Words
    | Tagging
    | WordTagAssociation

type Loader= DataKind -> string
type Saver= DataKind -> string -> unit


type CsvPersistenceStore(loader: Loader, saver: Saver)=
    let safeParse parse str= 
        match str with
        | null 
        | "" -> Array.empty
        | _ -> parse str

    interface IPersistenceStore with
        member this.loadPairs()= 
                Words |> loader |> safeParse CsvPair.ParseRows |> Array.map CsvPair.toSerializable |> List.ofArray |> Async.from
        member this.loadTags()= 
                Tagging |> loader |> safeParse CsvTag.ParseRows |> Array.map CsvTag.toSerializable |> List.ofArray |> Async.from
        member this.loadAssociations()= 
            WordTagAssociation |> loader 
                    |> safeParse CsvTagPairAssociation.ParseRows 
                    |> Array.map CsvTagPairAssociation.toSerializable 
                    |> List.ofArray |> Async.from

        member this.savePairs pairs= 
            let csv= pairs |> List.map CsvPair.fromSerializable
            (new CsvPair(csv)).SaveToString() |> saver Words |> Async.from

        member this.saveTags tags= 
            let csv= tags |> List.map CsvTag.fromSerializable
            (new CsvTag(csv)).SaveToString() |> saver Tagging |> Async.from

        member this.saveAssociations associations= 
            let csv= associations |> List.map CsvTagPairAssociation.fromSerializable
            (new CsvTagPairAssociation(csv)).SaveToString() |> saver WordTagAssociation |> Async.from

        member this.loadConfig()= 
            match Configuration |> loader |> safeParse CsvConfiguration.ParseRows |> List.ofArray with
                | [] -> CsvConfiguration.Row("", "")
                | head::_ -> head
                |> CsvConfiguration.toSerializable |> Async.from

        member this.saveConfig config= 
            let csv= config |> CsvConfiguration.fromSerializable |> Seq.singleton 
            (new CsvConfiguration(csv)).SaveToString() |> saver Configuration  |> Async.from
    
