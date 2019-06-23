namespace OldMan.LanguageTraining.Web

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Clients
open OldMan.LanguageTraining.Domain
type DomainTag= OldMan.LanguageTraining.Domain.Tag

module Tag=
    type State= 
        {
            Id: Id
            Text: string
            UsageCount: int
        }

    let idOf state= state.Id

    let empty()= 
        {
            Id= Id.uninitialized
            Text= ""
            UsageCount= 0 
        }

    let fromDomain ((tag: DomainTag), usageCount)= 
        {
            Id= tag.Id
            Text= tag.Text
            UsageCount= usageCount
        }


    [<NamedUnionCases "type">]
    type Message = 
        | Nil

    let update msg state=
        state

    let render (dispatch: Message Dispatch) (state: View<State>)=
        Templates.TagListRow().Text(state.V.Text).UsageCount(string state.V.UsageCount).Doc()

module TagList=
    type State= 
        {
            Tags: Tag.State list
        }
    let refresh()= 
        {
            Tags= Api.service().ListTags() |> List.map Tag.fromDomain
        }
    let delete id state=
        let without= state.Tags |> List.filter (fun t -> t.Id<>id)
        { state with Tags=without }

    [<NamedUnionCases "type">]
    type Message = 
        | Refresh
        | Delete of Id

    let init()= refresh()

    let update msg state=
        match msg with
        | Refresh -> refresh()
        | Delete id -> delete id state

    let render (dispatch: Message Dispatch) (state: View<State>)=
        let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
        Templates.TagList().Body(tags).Doc()


