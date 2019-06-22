[<WebSharper.JavaScript>]
module OldMan.LanguageTraining.Web.Site

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Web


type Endpoint=
    | [<EndPoint "/tags">] TagListPage
    | [<EndPoint "/">] WelcomePage
    | [<EndPoint "/other">] OtherPage

type State=
    {
        Endpoint : Endpoint
        Tags: Tag.Model list
    }

let init()= 
    {
        Endpoint= WelcomePage
        Tags= Tag.sample()
    }

[<NamedUnionCases "type">]
type Message =
    | Nil

let update msg (model: State)=
    match msg with
    | _ -> model    


let welcomePage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
    Templates.Welcome().Doc()
)

let otherPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
    Templates.Other().Doc()
)

let tagListPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
    let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
    Templates.TagList().Body(tags).Doc()
)

let page (state: State)=
    match state.Endpoint with
    | WelcomePage -> welcomePage()
    | OtherPage -> otherPage()
    | TagListPage -> tagListPage()


