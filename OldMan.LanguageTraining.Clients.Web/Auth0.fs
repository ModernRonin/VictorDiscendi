module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript


let private domain= "oldman.eu.auth0.com" 
let private clientId= "PCXHj3vHt1gCjgVkdYwRuUHmQtt8s11v"

type LoginOptions= 
    {
        [<Name("redirect_uri")>]
        RedirectUrl: string
    }
module LoginOptions=
    let create()= 
        {
            RedirectUrl= JS.Window.Origin + "#/auth/loggedin"
        }

type LogoutOptions=
    {
        [<Name("returnTo")>]
        ReturnUrl: string
    }
module LogoutOptions=
    let create()=
        {
            ReturnUrl= JS.Window.Origin + "#/auth/loggedout"
        }

type Auth()=
    [<Name("isAuthenticated")>]
    [<Stub>]
    member this.IsLoggedIn(): Promise<bool>= X<_> 
    [<Name("loginWithRedirect")>]
    [<Stub>]
    member this.Login(options: LoginOptions): Promise<unit>= X<_>
    [<Name("logout")>]
    [<Stub>]
    member this.Logout(options: LogoutOptions): Promise<unit>= X<_>
    [<Name("handleRedirectCallback")>]
    [<Stub>]
    member this.finishLogin() : Promise<unit>= X<_>

type CreateOptions=
    {
        [<Name("domain")>]
        Domain: string
        [<Name("client_id")>]
        ClientId: string
    }

[<Direct("createAuth0Client($options)")>]
let create(options: CreateOptions): Promise<Auth>= X<_>

let mutable private auth: Auth option= None
let mutable private _isLoggedIn= false



let private createClient()=
    async{
        let! result= create({Domain= domain; ClientId=clientId}).AsAsync()
        return result
    }

let private ensureClient()=
    async {
        match auth with 
        | None -> 
            let! a= createClient()
            auth <- Some a
        | _ -> ()
        return auth |> Option.get
    }

let private updateAuthenticationStatus()= 
    async {
        Console.Log "ensuring auth client"
        let! auth= ensureClient()
        Console.Log "checking login status"
        Console.Log auth
        let! result= auth.IsLoggedIn().AsAsync()
        _isLoggedIn <- result
        Console.Log ("isLoggedIn: " + string result)
    }

let setup= updateAuthenticationStatus

let isLoggedIn()= _isLoggedIn

let login()=
    async {
        let! auth= ensureClient()
        do! auth.Login(LoginOptions.create()).AsAsync()
    }

let logout()=
    async {
        let! auth= ensureClient()
        do! auth.Logout(LogoutOptions.create()).AsAsync()
        _isLoggedIn <- false
    }


let finishLogin()=
    async {
        let! auth= ensureClient()
        do! auth.finishLogin().AsAsync()
        JS.Window.History.ReplaceState(new JSObject(), JS.Document.Title, "/");
        do! updateAuthenticationStatus()
    }
