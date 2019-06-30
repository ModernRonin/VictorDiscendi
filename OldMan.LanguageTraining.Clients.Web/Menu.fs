module OldMan.LanguageTraining.Web.Menu

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Web

type State=
    {
        IsLoggedIn: bool
        Username: string
        // maybe: isLoading
    }


[<NamedUnionCases "type">]
type Message = 
    | Login
    | Logout

let init()=
    {
        IsLoggedIn= false
        Username= ""
    }

let update msg state=
    match msg with
    | Login -> 
        Auth0.login() |> ignore
        state
    | Logout -> state

let render (dispatch: Message Dispatch) (state: View<State>)=
    let notice state= 
        match state.IsLoggedIn with
        | true -> sprintf "Welcome, %s!" state.Username
        | false ->  "Please login!"
        
    Templates.Menu()
        .Login(fun _ -> dispatch Login)
        .Logout(fun _ -> dispatch Logout)
        .loginStateNotice(state.V |> notice)
        .Doc()