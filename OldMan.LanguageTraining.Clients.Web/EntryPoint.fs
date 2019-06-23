module OldMan.LanguageTraining.Clients.Web

open WebSharper
open WebSharper.Mvu
open WebSharper.Sitelets.InferRouter
open WebSharper.UI
open WebSharper.UI.Client

open OldMan.LanguageTraining.Web

[<SPAEntryPoint>]
let Main () =
    let router = Router.Infer<Site.Screen>()

    App.CreatePaged (Site.init()) Site.update Site.pageFor
    |> App.WithCustomRouting router (fun s -> s.Screen) Site.goto
#if DEBUG
    |> App.WithLocalStorage "VictorDiscendisDev"
    |> App.WithRemoteDev (RemoteDev.Options(hostname = "localhost", port = 8000))
#endif
    |> App.Run
    |> Doc.RunById "site"


[<assembly: WebSharper.JavaScript>]
do()
