module OldMan.LanguageTraining.Clients.Web

open WebSharper
open WebSharper.JavaScript
open WebSharper.Mvu
open WebSharper.Sitelets.InferRouter
open WebSharper.UI
open WebSharper.UI.Client

open OldMan.LanguageTraining.Web

[<SPAEntryPoint>]
let Main () =
    let router = Router.Infer<Site.Route>()
    let ol dispatch= dispatch |> Site.authDispatch |> Authentication.onLoad

    App.CreatePaged (Site.init()) Site.update Site.pageFor
    |> App.WithCustomRouting router Site.routeForState Site.goto
    |> App.WithInitAction (CommandAsync ol)
#if DEBUG
    //|> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
#endif
    |> App.Run
    |> Doc.RunById "site"


[<assembly: WebSharper.JavaScript>]
do()
