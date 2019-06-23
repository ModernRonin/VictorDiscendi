namespace OldMan.LanguageTraining.Web

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Domain

module Tag=
    type Model= 
        {
            Id: Id
            Text: string
            UsageCount: int
        }

    let idOf model= model.Id

    //[<NamedUnionCases "type">]
    type Message = 
        | Nil

    let update msg model=
        model

    let render (dispatch: Message Dispatch) (state: View<Model>)=
        Templates.TagListRow().Text(state.V.Text).UsageCount(string state.V.UsageCount).Doc()

module TagList=
    type State= 
        {
            Tags: Tag.Model list
        }

    //[<NamedUnionCases "type">]
    type Message = 
        | Nil

    let init()=
        {
            Tags= [
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
                    {
                        Id= Id.wrap 3L
                        Text="charlie"
                        UsageCount=19
                    }
                ]
        }

    let update msg model=
        model

    let render (dispatch: Message Dispatch) (state: View<State>)=
        let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
        Templates.TagList().Body(tags).Doc()


