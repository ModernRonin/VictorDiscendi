module OldMan.LanguageTraining.Web.Tag

open WebSharper
open WebSharper.UI

open OldMan.LanguageTraining.Domain

type Model= 
    {
        Id: Id
        Text: string
        UsageCount: int
    }
let idOf model= model.Id
let sample()= 
    [
        {
            Id= Id.wrap 1L
            Text="alpha"
            UsageCount=13
        }
        {
            Id= Id.wrap 2L
            Text="bravo"
            UsageCount=17
        }
    ]
[<NamedUnionCases "type">]
type Message = 
    | Nil
let update msg model=
    model

let render (dispatch: Message -> unit) (state: View<Model>)=
    Templates.TagListRow().Text(state.V.Text).UsageCount(string state.V.UsageCount).Doc()



