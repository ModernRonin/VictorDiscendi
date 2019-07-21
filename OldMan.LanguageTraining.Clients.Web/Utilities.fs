namespace OldMan.LanguageTraining.Web

module Utilities=
    open WebSharper
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.Mvu
    
    let hiddenIf condition= condition |> Attr.ClassPred "hidden" 
    let visibleIf condition= not condition |> hiddenIf

        
    let subDispatch wrapper dispatch msg =
        msg |> wrapper |> dispatch
