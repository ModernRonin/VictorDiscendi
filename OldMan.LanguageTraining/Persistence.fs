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

