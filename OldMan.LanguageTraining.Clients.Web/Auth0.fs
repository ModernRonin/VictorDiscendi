module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript
open WebSharper.Mvu

[<Inline("AuthJS.login()")>]
let login(): Promise<unit>= X<_>
[<Inline("AuthJS.logout()")>]
let logout(): Promise<unit>= X<_>
[<Direct("AuthJS.getIsLoggedIn()")>]
let isLoggedIn(): bool= X<_>
[<Direct("AuthJS.onPageLoad()")>]
let onPageLoad(): Promise<unit>= X<_>

type Message=
    | UpdateLoggedInStatus of bool

let onLoad (dispatch: Message Dispatch)= 
    async {
        do! onPageLoad().AsAsync()
        let isLoggedIn= isLoggedIn()
        dispatch (UpdateLoggedInStatus isLoggedIn)
    }