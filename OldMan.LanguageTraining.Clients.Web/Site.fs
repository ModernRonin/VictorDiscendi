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

let authDispatch dispatch msg= 
    let wrap msg= (AuthMessage msg)
    Utilities.subDispatch wrap dispatch msg

let tagsDispatch dispatch msg=
    let wrap msg= (TagListMessage msg)
    Utilities.subDispatch wrap dispatch msg

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
                Authentication.onLoad (authDispatch dispatch)
            CommandAsync onLoad
        | Authentication.UpdateLoggedInStatus isLoggedIn ->
            let userInfo= Authentication.updateState isLoggedIn
            SetModel {state with UserInfo=userInfo}

let render (dispatch: Message Dispatch) (state: View<State>)=
    let authDispatch = authDispatch dispatch
    let tagsDispatch = tagsDispatch dispatch

    let renderScreen (state: State)=
        match state.Screen with
        | WelcomeScreen -> Templates.Welcome().Doc()
        | OtherScreen -> Templates.Other().Doc()
        | TagListScreen ->
            Tags.render tagsDispatch (V state.TagList)

    let renderAuth= 
        (Authentication.render authDispatch (V state.V.UserInfo)).Doc()
    Templates.Menu()
        .UserInfo(renderAuth)
        .Screen(state.Doc renderScreen)
        .Doc()


let pageFor _ = (Page.Single render)()

let goto (route: Route) (state: State) : State =
    match route with
    | Welcome -> {state with Route=route; Screen= WelcomeScreen}
    | Other -> {state with Route=route; Screen= OtherScreen}
    | TagList -> {state with Route=route; Screen= TagListScreen; TagList= Tags.refresh()}


let routeForState state = state.Route
    