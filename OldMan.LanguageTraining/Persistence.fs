namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

type IPersistence=
    abstract member UpdateConfiguration: LanguageConfiguration->unit
    abstract member GetConfiguration : unit->LanguageConfiguration
    abstract member AddPair: WordPair -> WordPair
    abstract member UpdatePair: WordPair -> unit
    abstract member GetPairs: unit -> WordPair list
    abstract member GetTags: unit -> Tag list
    abstract member AddOrUpdateTag: Tag -> Tag

type SerializableConfiguration=
    {
        LeftName: string
        RightName: string
    }

type SerializablePair=
    {
        Id: int64
        Left: string
        Right: string
        Created: string
        LastAsked: string
        TimesAsked: int64
        LeftScore: int
        RightScore:int
    }

type SerializableTag=
    {
        Id: int64
        Text: string
    }

type SerializableTagPairAssociation=
    {
        TagId: int64
        PairId: int64
    }