module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript
open WebSharper.Mvu

[<Inline("AuthJS.login()")>]
let private doLogin(): Promise<unit>= X<_>
[<Inline("AuthJS.logout()")>]
let private doLogout(): Promise<unit>= X<_>
[<Direct("AuthJS.getIsLoggedIn()")>]
let private isLoggedIn(): bool= X<_>
[<Direct("AuthJS.onPageLoad()")>]
let private onPageLoad(): Promise<unit>= X<_>

type Message=
    | UpdateLoggedInStatus of bool

let update (dispatch: Message Dispatch)= 
    async {
        do! onPageLoad().AsAsync()
        let isLoggedIn= isLoggedIn()
        dispatch (UpdateLoggedInStatus isLoggedIn)
    }

let login()= CommandAsync(fun _ -> doLogin().AsAsync())
let logout()= CommandAsync(fun _ -> doLogout().AsAsync())