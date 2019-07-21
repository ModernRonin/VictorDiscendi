module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript
open WebSharper.Mvu
open WebSharper.UI
open WebSharper.UI.Client

open OldMan.LanguageTraining.Web

[<Stub>]
type UserInformation= 
    {
        [<Name("nickname")>]
        Nickname: string
        [<Name("name")>]
        Name: string
        [<Name("picture")>]
        ImageUrl: string
        [<Name("email")>]
        Email: string
        [<Name("email_verified")>]
        IsEmailVerified: bool
    }

[<Inline("AuthJS.login()")>]
let private doLogin(): Promise<unit>= X<_>
[<Inline("AuthJS.logout()")>]
let private doLogout(): Promise<unit>= X<_>
[<Direct("AuthJS.getIsLoggedIn()")>]
let private isLoggedIn(): bool= X<_>
[<Direct("AuthJS.onPageLoad()")>]
let private onPageLoad(): Promise<unit>= X<_>
[<Direct("AuthJS.getUserInfo()")>]
let private getUserInfo(): UserInformation= X<_>

type Message=
    | UpdateLoggedInStatus of bool
    | Login
    | Logout
    | CheckForCallbacks

type State=
    {
        IsLoggedIn: bool
        Username: string 
        ImageUrl: string
    }
let init()=
    {
        IsLoggedIn= false
        Username= ""
        ImageUrl= ""
    }

let login()= CommandAsync(fun _ -> doLogin().AsAsync())
let logout()= CommandAsync(fun _ -> doLogout().AsAsync())
let onLoad (dispatch: Message Dispatch)= 
    async {
        do! onPageLoad().AsAsync()
        let isLoggedIn= isLoggedIn()
        dispatch (UpdateLoggedInStatus isLoggedIn)
    }
let checkForCallback()= CommandAsync onLoad

let updateState isLoggedIn=
    match isLoggedIn with
    | false -> init()
    | true ->
        let userInfo= getUserInfo()
        {
            IsLoggedIn= true
            Username= userInfo.Name
            ImageUrl= userInfo.ImageUrl
        }



// doesn't work because it returns an Action
// so the calling module can't use that to update it's sub-state
// see https://forums.websharper.com/topic/87100
let update msg _=
    match msg with
    | Login -> login() 
    | Logout -> logout()
    | CheckForCallbacks -> checkForCallback()
    | UpdateLoggedInStatus isLoggedIn ->
        SetModel (updateState isLoggedIn)

let render (dispatch: Message Dispatch) (state: View<State>)=
    let notice state= 
        match state.IsLoggedIn with
        | true -> sprintf "Welcome, %s!" state.Username
        | false ->  "Please login!"

    let avatar=
        let imageSrc= Attr.Dynamic "src" (V state.V.ImageUrl)
        let isHidden= Attr.DynamicClassPred "hidden" (V (not state.V.IsLoggedIn))
        Attr.Append imageSrc isHidden

    Templates.UserInfo()
        .Login(fun _ -> dispatch Login)
        .Logout(fun _ -> dispatch Logout)
        .LoginStateNotice(state.V |> notice)
        .LoginAttributes(Attr.ClassPred "hidden" state.V.IsLoggedIn)
        .LogoutAttributes(Attr.ClassPred "hidden" (not state.V.IsLoggedIn)) 
        .AvatarAttributes(avatar)
        //.Avatar(state.V.ImageUrl)
        //.AvatarVisibility(Attr.DynamicClassPred "hidden" (state |> View.Map (fun s -> not s.IsLoggedIn)))
