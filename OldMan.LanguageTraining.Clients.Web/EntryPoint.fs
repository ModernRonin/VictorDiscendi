[<WebSharper.JavaScript>]
module OldMan.LanguageTraining.Clients.Web

open WebSharper
open WebSharper.Mvu
open WebSharper.Sitelets.InferRouter
open WebSharper.UI
open WebSharper.UI.Client


open OldMan.LanguageTraining.Web

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


