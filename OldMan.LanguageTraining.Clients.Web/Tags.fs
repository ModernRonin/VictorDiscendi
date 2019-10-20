module OldMan.LanguageTraining.Web.Tags

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Clients
open OldMan.LanguageTraining.Domain

type State= 
    {
        Tags: (Tag*int) list
    }
let refresh()= 
    async {
        let! tags=  Api.service().ListTags()
        return {
            Tags= tags
        }
     }

let delete id state=
    let without= state.Tags |> List.filter (fun (t, _) -> t.Id<>id)
    { state with Tags=without }

[<NamedUnionCases "type">]
type Message = 
    | Refresh
    | Delete of Id

let init()= 
    {
        Tags= []
    }

let update msg state=
    match msg with
    | Refresh -> refresh()
    | Delete id -> delete id state

let render (dispatch: Message Dispatch) (state: View<State>)=
    let renderTag id (view: View<Tag*int>)= 
        let tagView= view |> View.MapCached (fun (t, _) -> t)
        let countView= view |> View.MapCached (fun (_, c) -> c)
        Templates.TagListRow()
            .Text(tagView.V.Text)
            .UsageCount(string countView.V)
            .Delete(fun _ -> dispatch (Delete id))
            .Doc()
    let idOf (tag, _)= tag.Id
    let tags= (V (state.V.Tags)).DocSeqCached(idOf, renderTag) |> Seq.singleton
    Templates.TagList()
        .Body(tags)
        .Refresh(fun _ -> dispatch Refresh)
        .Doc()


