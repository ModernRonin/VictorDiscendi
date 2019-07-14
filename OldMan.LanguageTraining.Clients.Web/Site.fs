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
    | [<EndPoint "/auth/loggedin">] AuthLoggedIn
    | [<EndPoint "/auth/loggedout">] AuthLoggedOut

[<NamedUnionCases "type">]
type Screen=
    | TagListScreen 
    | WelcomeScreen
    | OtherScreen

type State=
    {
        IsLoggedIn: bool
        Username: string
        // maybe: isLoading
        Screen : Screen
        Route: Route
        TagList: Tags.State
    }

let init()= 
    {
        IsLoggedIn= false
        Username= ""
        Route= Welcome
        Screen= WelcomeScreen
        TagList= Tags.init()
    }


[<NamedUnionCases "type">]
type Message =
    | TagListMessage of Tags.Message
    | Login
    | Logout

let updateLoginStatus state= 
    {state with IsLoggedIn= Authentication.isLoggedIn()}

let update msg (state: State) : Action<Message, State> =
    match msg with
    | TagListMessage m -> 
        let updatedTagList= Tags.update m state.TagList
        SetModel {state with TagList=updatedTagList}
    | Login -> CommandAsync(fun _ -> Authentication.login().AsAsync())
    | Logout -> 
        //Authentication.logout() |> ignore
        //Action.DoNothing
        CommandAsync(fun _ -> Authentication.logout().AsAsync())

let render (dispatch: Message Dispatch) (state: View<State>)=
    let notice state= 
        match state.IsLoggedIn with
        | true -> sprintf "Welcome, %s!" state.Username
        | false ->  "Please login!"
    let inline isLoggedIn state= state.IsLoggedIn  
    let visibleIfLoggedIn state=
        match state.IsLoggedIn with
        | true -> Attr.ClassPred "hidden" false
        | false -> Attr.ClassPred "hidden" true

    let renderScreen (state: State)=
        let delegateToComponent renderer stateExtractor transformer=
            let subDispatch msg = dispatch (transformer msg)
            renderer subDispatch (View.Const (stateExtractor state))
        match state.Screen with
        | WelcomeScreen -> Templates.Welcome().Doc()
        | OtherScreen -> Templates.Other().Doc()
        | TagListScreen ->
            delegateToComponent Tags.render (fun s -> s.TagList) (fun m -> TagListMessage m)

    
    Templates.Menu()
        .Login(fun _ -> dispatch Login)
        .Logout(fun _ -> dispatch Logout)
        .LoginAttributes(Attr.ClassPred "hidden" state.V.IsLoggedIn)
        .LogoutAttributes(Attr.ClassPred "hidden" (not state.V.IsLoggedIn)) 
        .LoginStateNotice(state.V |> notice)
        .Screen((V (state.V)).Doc renderScreen)
        .Doc()


let pageFor _ = (Page.Single render)()

let goto (route: Route) (state: State) : State =
    match route with
    | Welcome -> {state with Route=route; Screen= WelcomeScreen}
    | Other -> {state with Route=route; Screen= OtherScreen}
    | TagList -> {state with Route=route; Screen= TagListScreen; TagList= Tags.refresh()}
    | AuthLoggedIn -> 
        {state with IsLoggedIn= true}
    | AuthLoggedOut -> 
        {state with IsLoggedIn= false; Screen= WelcomeScreen}


let routeForState state = state.Route
    