module OldMan.LanguageTraining.Web.Auth0

open WebSharper
open WebSharper.JavaScript

[<Direct("window.Auth0Wrapper.login()")>]
let login ()= X<Promise<unit>>

[<Direct("window.Auth0Wrapper.isAuthenticated")>]
let isLoggedIn= X<bool>

