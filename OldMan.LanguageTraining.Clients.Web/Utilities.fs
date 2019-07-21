namespace OldMan.LanguageTraining.Web

[<AutoOpen>]
module Utilities=
    open WebSharper
    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.Mvu
    
    let hiddenIf (condition: View<bool>)= 
        Attr.DynamicClassPred "hidden" condition
    let visibleIf (condition: View<bool>)= 
        condition |> View.Map not |> hiddenIf

        
    let subDispatch wrapper dispatch msg =
        msg |> wrapper |> dispatch
