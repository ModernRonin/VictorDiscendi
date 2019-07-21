module OldMan.LanguageTraining.Web.Site

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Web

[<NamedUnionCases "type">]
type Route= 
    | [<EndPoint "/tags">] TagList
    | [<EndPoint "/">] Welcome
    | [<EndPoint "/other">] Other

[<NamedUnionCases "type">]
type Screen=
    | TagListScreen 
    | WelcomeScreen
    | OtherScreen

type State=
    {
        UserInfo: Authentication.State
        // maybe: isLoading
        Screen : Screen
        Route: Route
        TagList: Tags.State
    }

let init()= 
    {
        Route= Welcome
        Screen= WelcomeScreen
        TagList= Tags.init()
        UserInfo= Authentication.init()
    }


[<NamedUnionCases "type">]
type Message =
    | TagListMessage of Tags.Message
    | AuthMessage of Authentication.Message

let update msg (state: State) : Action<Message, State> =
    match msg with
    | TagListMessage m -> 
        let updatedTagList= Tags.update m state.TagList
        SetModel {state with TagList=updatedTagList}
    | AuthMessage m ->
        match m with 
        | Authentication.Login -> Authentication.login()
        | Authentication.Logout -> Authentication.logout()
        | Authentication.CheckForCallbacks -> 
            let onLoad (dispatch: Message Dispatch)=
                Authentication.onLoad (fun m2 -> dispatch (AuthMessage m2))
            CommandAsync onLoad
        | Authentication.UpdateLoggedInStatus isLoggedIn ->
            let userInfo= Authentication.updateState isLoggedIn
            SetModel {state with UserInfo=userInfo}

let render (dispatch: Message Dispatch) (state: View<State>)=
    let renderScreen (state: State)=
        let delegateToComponent renderer stateExtractor transformer=
            let subDispatch msg = dispatch (transformer msg)
            renderer subDispatch (View.Const (stateExtractor state))
        match state.Screen with
        | WelcomeScreen -> Templates.Welcome().Doc()
        | OtherScreen -> Templates.Other().Doc()
        | TagListScreen ->
            delegateToComponent Tags.render (fun s -> s.TagList) (fun m -> TagListMessage m)

    let d2 m= dispatch (AuthMessage m)
    let renderAuth= 
        (Authentication.render d2 (V (state.V.UserInfo))).Doc()
    Templates.Menu()
        .UserInfo(renderAuth)
        .Screen((V (state.V)).Doc renderScreen)
        .Doc()


let pageFor _ = (Page.Single render)()

let goto (route: Route) (state: State) : State =
    match route with
    | Welcome -> {state with Route=route; Screen= WelcomeScreen}
    | Other -> {state with Route=route; Screen= OtherScreen}
    | TagList -> {state with Route=route; Screen= TagListScreen; TagList= Tags.refresh()}


let routeForState state = state.Route
    