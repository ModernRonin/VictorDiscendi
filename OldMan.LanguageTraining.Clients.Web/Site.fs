[<WebSharper.JavaScript>]
module OldMan.LanguageTraining.Clients.Web

open WebSharper
open WebSharper.JavaScript
open WebSharper.Sitelets.InferRouter
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Domain
open OldMan.LanguageTraining.Web.Templates

module Tag=
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
        MainTemplate.Row().Text(state.V.Text).UsageCount(string state.V.UsageCount).Doc()


module Site =    
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
        MainTemplate.Welcome().Doc()
    )

    let otherPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
        MainTemplate.Other().Doc()
    )

    let tagListPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
        let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
        MainTemplate.TagList().Body(tags).Doc()
    )

    let page (state: State)=
        match state.Endpoint with
        | WelcomePage -> welcomePage()
        | OtherPage -> otherPage()
        | TagListPage -> tagListPage()


[<SPAEntryPoint>]
let Main () =
    App.CreateSimplePaged (Site.init()) Site.update Site.page
    |> App.WithRouting (Router.Infer()) (fun (model: Site.State) -> model.Endpoint)
#if DEBUG
    |> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
#endif
    |> App.Run
    |> Doc.RunById "site"
