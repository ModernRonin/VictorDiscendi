module OldMan.LanguageTraining.Web.Authentication

open WebSharper
open WebSharper.JavaScript

type LoginOptions= 
    {
        [<Name("redirect_uri")>]
        RedirectUrl: string
    }
module LoginOptions=
    let create()= 
        {
            RedirectUrl= JS.Window.Location.Href
        }

type LogoutOptions=
    {
        [<Name("returnTo")>]
        ReturnUrl: string
    }
module LogoutOptions=
    let create()=
        {
            ReturnUrl= JS.Window.Location.Origin
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

let setup domain clientId=
    async {
        Console.Log "Creating client..."
        let! a= create({Domain= domain; ClientId=clientId}).AsAsync()
        Console.Log "client created"
        auth <- Some a
        Console.Log "client set"
        Console.Log auth
    }

let isLoggedIn()= 
    match auth with
    | None -> failwith "setup not called or awaited"
    | Some a ->
        async {
            return! a.IsLoggedIn().AsAsync()
        }

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
        }

