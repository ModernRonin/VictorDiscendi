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
            do! Auth0.login().AsAsync()
        })
    | Logout -> DoNothing

let render (dispatch: Message Dispatch) (state: View<State>)=
    let notice state= 
        match state.IsLoggedIn with
        | true -> sprintf "Welcome, %s!" state.Username
        | false ->  "Please login!"
        
    let renderScreen (state: State)=
        match state.Screen with
        | WelcomeScreen -> Templates.Welcome().Doc()
        | OtherScreen -> Templates.Other().Doc()
        | TagListScreen ->
            let subDispatch msg= dispatch (TagListMessage msg)
            Tags.render subDispatch (View.Const state.TagList)

    Templates.Menu()
        .Login(fun _ -> dispatch Login)
        .Logout(fun _ -> dispatch Logout)
        .loginStateNotice(state.V |> notice)
        .Screen((V (state.V)).Doc renderScreen)
        .Doc()


let pageFor (state: State)=
    let dataless docCreator= Page.Single(fun _ _ -> docCreator())
    let delegateToComponent renderer stateExtractor transformer=
        Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) -> 
            let subDispatch msg = dispatch (transformer msg)
            renderer subDispatch (V (stateExtractor state.V))
        )
    let pageCreator= 
        match state.Screen with
        | WelcomeScreen -> dataless (fun () -> Templates.Welcome().Doc())
        | OtherScreen -> dataless (fun () -> Templates.Other().Doc())
        | TagListScreen -> delegateToComponent Tags.render (fun s -> s.TagList) (fun msg -> (TagListMessage msg))

    (Page.Single render)()
    //pageCreator()

let goto (screen: Screen) (state: State) : State =
    match screen with
    | WelcomeScreen 
    | OtherScreen -> {state with Screen=screen}
    | TagListScreen -> 
        {state with Screen=screen; TagList= Tags.refresh()}