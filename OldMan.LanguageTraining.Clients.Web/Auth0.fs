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


let updateAuthenticationStatus()= 
    match auth with
    | None -> failwith "setup not called or awaited"
    | Some a ->
        async {
            Console.Log "updating login status..."
            let! result= a.IsLoggedIn().AsAsync()
            _isLoggedIn <- result
            Console.Log ("isLoggedIn=" + string _isLoggedIn)
        }

let createClient()=
    async{
        Console.Log "Creating client..."
        let! result= create({Domain= domain; ClientId=clientId}).AsAsync()
        Console.Log "client created"
        return result
    }

let setup()= 
    async {
        let! a= createClient()
        auth <- Some a
        Console.Log "client set"
        Console.Log auth
        do! updateAuthenticationStatus()
    }

let ensureClient()=
    async {
        match auth with 
        | None -> do! setup()
        | _ -> ()
        return auth |> Option.get
    }

let isLoggedIn()= _isLoggedIn

let login()=
    match auth with
    | None -> failwith "setup not called or awaited"
    | Some a ->
        async {
            do! a.Login(LoginOptions.create()).AsAsync()
        }

let logout()=
    match auth with
    | None -> failwith "setup not called or awaited"
    | Some a ->
        async {
            do! a.Logout(LogoutOptions.create()).AsAsync()
            _isLoggedIn <- false
        }

let finishLogin()=
    async {
        let! auth= ensureClient()
        do! auth.finishLogin().AsAsync()
        do! updateAuthenticationStatus()
    }
