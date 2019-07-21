module OldMan.LanguageTraining.Web.Networking

open WebSharper
open WebSharper.JavaScript

type Verb=
    | Post
    | Get
    | Put
    | Delete

let private verbToMethod = function
    | Post -> "POST"
    | Get -> "GET"
    | Put -> "PUT"
    | Delete -> "DELETE"

module ContentType=
    let wwwFormUrlEncoded= 
        [("Content-Type", "application/x-www-form-urlencoded")]  |> Map.ofList


// uncomment this once the problem with Websharper and setting headers is clarified
//let fetch (verb: Verb) headers url (body: obj option)=
//    let o= new RequestOptions()
//    o.Method <- verb |> verbToMethod
//    let addHeader key value=
//        o.Headers.Append(key, value)
//    headers |> Map.iter addHeader
//    match (verb, body) with
//    | Post, Some b
//    | Put, Some b 
//        -> o.Body <- b
//    | _ -> ()
//    async {
//        let! response= JS.Fetch(url, o).AsAsync()
//        return response
//    }

[<Direct("fetch($url, $options)")>]
let private fetchWrapper(url: string, options: obj): Promise<Response>= X<_>

let fetch (verb: Verb) headers url (body: obj option)=
    let options= new JSObject()
    options.SetJS("method", verb |> verbToMethod)
    let h= new JSObject()
    headers |> Map.iter (fun key value -> h.SetJS(key, value))
    options.SetJS("headers", h)
    match (verb, body) with
    | Post, Some b
    | Put, Some b 
        -> options.SetJS("body", b)
    | _ -> ()
    async {
        let! response= fetchWrapper(url, options).AsAsync()
        return response
    }


let postForm= fetch Post ContentType.wwwFormUrlEncoded

