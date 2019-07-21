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
    let onLoad= CommandAsync (fun (dispatch: Site.Message Dispatch) -> 
                                let d1 m= dispatch (Site.AuthMessage m)
                                Authentication.update(d1))
    App.CreatePaged (Site.init()) Site.update Site.pageFor
    |> App.WithCustomRouting router Site.routeForState Site.goto
    |> App.WithInitAction onLoad
#if DEBUG
    |> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
#endif
    |> App.Run
    |> Doc.RunById "site"


[<assembly: WebSharper.JavaScript>]
do()
