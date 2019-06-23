module OldMan.LanguageTraining.Web.Site

open WebSharper
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
        Screen : Screen
        TagList: TagList.State
    }

let init()= 
    {
        Screen= WelcomeScreen
        TagList= TagList.init()
    }

//[<NamedUnionCases "type">]
type Message =
    | Nil

let update msg (model: State) : Action<Message, State> =
    match msg with
    | _ -> DoNothing 


let tagListPage= Page.Single(fun (dispatch: Dispatch<Message>) (state: View<State>) ->
    TagList.render ignore (V state.V.TagList)
)

let pageFor (state: State)=
    let dataless docCreator= Page.Single(fun _ _ -> docCreator())()
    
    match state.Screen with
    | WelcomeScreen -> dataless (fun () -> Templates.Welcome().Doc())
    | OtherScreen -> dataless (fun () -> Templates.Other().Doc())
    | TagListScreen -> tagListPage()


let goto (screen: Screen) (state: State) : State =
    match screen with
    | WelcomeScreen 
    | OtherScreen -> {state with Screen=screen}
    | TagListScreen -> {state with Screen=screen; TagList= TagList.init()}