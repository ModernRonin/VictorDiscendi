[<WebSharper.JavaScript>]
module OldMan.LanguageTraining.Web.Templates

open WebSharper.UI.Templating

type MainTemplate= Template<"wwwroot/index.html", ClientLoad.FromDocument>

