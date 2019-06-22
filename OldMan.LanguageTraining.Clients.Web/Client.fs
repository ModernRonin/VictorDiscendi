[<WebSharper.JavaScript>]
module TodoMvc.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.Sitelets.InferRouter
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Mvu

open OldMan.LanguageTraining.Domain

type MainTemplate= Template<"wwwroot/index.html", ClientLoad.FromDocument>
    

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


module Main =    
    type Endpoint=
        | [<EndPoint "/tags">] TagListPage
        | [<EndPoint "/">] WelcomePage
        | [<EndPoint "/other">] OtherPage

    type Model=
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

    let update msg (model: Model)=
        match msg with
        | _ -> model    


    let welcomePage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<Model>) ->
        MainTemplate.Welcome().Doc()
    )

    let otherPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<Model>) ->
        MainTemplate.Other().Doc()
    )

    let tagListPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<Model>) ->
        let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
        MainTemplate.TagList().Body(tags).Doc()
    )

    
    //let renderTagListPage (state:View<=
    //    let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
    //    MainTemplate.TagList().Body(tags).Doc()
        
    let page (state: Model)=
        match state.Endpoint with
        | WelcomePage -> welcomePage()
        | OtherPage -> otherPage()
        | TagListPage -> tagListPage()


[<SPAEntryPoint>]
let Main () =
    App.CreateSimplePaged (Main.init()) Main.update Main.page
    |> App.WithRouting (Router.Infer()) (fun (model: Main.Model) -> model.Endpoint)
    |> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
    |> App.Run
    |> Doc.RunById "main"
