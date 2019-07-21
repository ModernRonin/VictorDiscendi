namespace OldMan.LanguageTraining

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
    abstract member loadPairs: unit -> Async<SerializablePair list>
    abstract member loadTags: unit -> Async<SerializableTag list>
    abstract member loadAssociations: unit -> Async<SerializableTagPairAssociation list>
    abstract member loadConfig: unit -> Async<SerializableConfiguration>
    abstract member savePairs: SerializablePair list -> Async<unit>
    abstract member saveTags: SerializableTag list -> Async<unit>
    abstract member saveAssociations: SerializableTagPairAssociation list -> Async<unit>
    abstract member saveConfig: SerializableConfiguration -> Async<unit>
