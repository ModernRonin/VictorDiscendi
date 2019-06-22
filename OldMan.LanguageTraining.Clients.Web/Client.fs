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
    
let log x = Console.Log x

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
        (sprintf "Tag.update with %A" model) |> log
        model

    let render (dispatch: Message -> unit) (state: View<Model>)=
        (sprintf "Tag.render with %A" state) |> log
        MainTemplate.Row().Text(state.V.Text + "_tpl").UsageCount(string state.V.UsageCount).Doc()


module Main =    
    type Model =
        {
            Tags: Tag.Model list
        }

    let init() =
        {
            Tags= Tag.sample()
        }

    [<NamedUnionCases "type">]
    type Message =
        | Nil

    let update msg (model: Model)=
        (sprintf "Main.update with %A" model) |> log
        match msg with
        | _ -> model    


    let render (dispatch: Message -> unit) (state: View<Model>)=
        (sprintf "xMain.render with %A" state) |> log
        //MainTemplate
        //    .Table()
        //    .Body((V state.V.Tags).DocSeqCached(Tag.idOf, (fun id tagModel ->  Tag.render ignore tagModel)))
        //    .Doc()

        let tags= (V (state.V.Tags)).DocSeqCached(Tag.idOf, fun id t -> Tag.render ignore t) |> Seq.singleton
        div [] [
               h1 [] [ text "Tag List" ]
               table [] [
                    thead [] [
                        tr [] [
                            th [] [ text "Name" ]
                            th [] [ text "Count"]
                        ]
                    ]
                    tbody [] tags
               ]
               ul [] [
                   li [] [ text "..." ]
               ]
           ]


[<SPAEntryPoint>]
let Main () =
    App.CreateSimple (Main.init()) Main.update Main.render
//    |> App.WithRouting (Router.Infer()) (fun (model: TodoList.Model) -> model.EndPoint)
    |> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
    |> App.Run
    |> Doc.RunById "main"
