module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript


[<Inline("AuthJS.login()")>]
let login(): Promise<unit>= X<_>
[<Inline("AuthJS.logout()")>]
let logout(): Promise<unit>= X<_>
[<Direct("AuthJS.getIsLoggedIn()")>]
let isLoggedIn(): bool= X<_>
