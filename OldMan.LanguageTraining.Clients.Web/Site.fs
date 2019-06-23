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

[<NamedUnionCases "type">]
type Message =
    | TagListMessage of TagList.Message

let update msg (state: State) : Action<Message, State> =
    match msg with
    | TagListMessage m -> 
        let updatedTagList= TagList.update m state.TagList
        SetModel {state with TagList=updatedTagList}
    | _ -> DoNothing 



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
        | TagListScreen -> delegateToComponent TagList.render (fun s -> s.TagList) (fun msg -> (TagListMessage msg))

    pageCreator()

let goto (screen: Screen) (state: State) : State =
    match screen with
    | WelcomeScreen 
    | OtherScreen -> {state with Screen=screen}
    | TagListScreen -> 
        {state with Screen=screen; TagList= TagList.refresh()}