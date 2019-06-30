module OldMan.LanguageTraining.Web.Templates

open WebSharper.UI.Templating

type SiteTemplate= Template<"wwwroot/index.html", ClientLoad.FromDocument>
let Welcome()= SiteTemplate.Welcome()
let Other()= SiteTemplate.Other()
let TagList()= SiteTemplate.TagList()
let TagListRow() = SiteTemplate.Row()
let Menu()= SiteTemplate.Menu()
