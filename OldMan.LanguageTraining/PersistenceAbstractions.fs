namespace OldMan.LanguageTraining

open OldMan.LanguageTraining.Domain

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

type IPersistenceStore=
    abstract member loadPairs: unit -> SerializablePair list
    abstract member loadTags: unit -> SerializableTag list
    abstract member loadAssociations: unit -> SerializableTagPairAssociation list
    abstract member loadConfig: unit -> SerializableConfiguration
    abstract member savePairs: SerializablePair list -> unit
    abstract member saveTags: SerializableTag list -> unit
    abstract member saveAssociations: SerializableTagPairAssociation list -> unit
    abstract member saveConfig: SerializableConfiguration -> unit
