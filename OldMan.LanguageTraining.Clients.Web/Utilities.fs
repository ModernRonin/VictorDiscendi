namespace OldMan.LanguageTraining.Web

[<AutoOpen>]
module Utilties=
    open WebSharper
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.Mvu
    
    let hiddenIf condition= condition |> Attr.ClassPred "hidden" 
    let visibleIf condition= not condition |> hiddenIf

        