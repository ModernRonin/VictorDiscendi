module OldMan.LanguageTraining.Web.Site

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.Mvu

open OldMan.LanguageTraining.Web


type Screen=
    | [<EndPoint "/tags">] TagListScreen 
    | [<EndPoint "/">] WelcomeScreen
    | [<EndPoint "/other">] OtherScreen

type State=
    {
        IsLoggedIn: bool
        Username: string
        // maybe: isLoading
        Screen : Screen
        TagList: Tags.State
    }

let init()= 
    async {
        do! Authentication.setup "oldman.eu.auth0.com" "PCXHj3vHt1gCjgVkdYwRuUHmQtt8s11v"
    }
    {
        IsLoggedIn= false
        Username= ""
        Screen= WelcomeScreen
        TagList= Tags.init()
    }


[<NamedUnionCases "type">]
type Message =
    | TagListMessage of Tags.Message
    | Login
    | Logout

let update msg (state: State) : Action<Message, State> =
    match msg with
    | TagListMessage m -> 
        let updatedTagList= Tags.update m state.TagList
        SetModel {state with TagList=updatedTagList}
    | Login -> 
        CommandAsync (fun _ -> async {
            do! Authentication.login()
        })
    | Logout -> 
        CommandAsync (fun _ -> async {
            do! Authentication.logout()
        })



let render (dispatch: Message Dispatch) (state: View<State>)=
    let notice state= 
        match state.IsLoggedIn with
        | true -> sprintf "Welcome, %s!" state.Username
        | false ->  "Please login!"
    let isLoggedIn state= state.IsLoggedIn  

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
        //.LoginAttributes(state.V |> isLoggedIn |> hiddenIf)
        //.LogoutAttributes(state.V |> isLoggedIn |> visibleIf)
        .LoginStateNotice(state.V |> notice)
        .Screen((V (state.V)).Doc renderScreen)
        .Doc()


let pageFor _ = (Page.Single render)()

let goto (screen: Screen) (state: State) : State =
    match screen with
    | WelcomeScreen 
    | OtherScreen -> {state with Screen=screen}
    | TagListScreen -> 
        {state with Screen=screen; TagList= Tags.refresh()}