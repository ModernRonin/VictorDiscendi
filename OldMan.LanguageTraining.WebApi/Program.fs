module OldMan.LanguageTraining.WebApi

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

let webApp =
    choose [
        route "/ping"   >=> text "pong"
        route "/"       >=> htmlFile "/index.html" ]

let configureApp (app : IApplicationBuilder) =
    app.UseStaticFiles() 
        .UseAuthentication()
        .UseGiraffe webApp |> ignore

let configureServices (services : IServiceCollection) =
    services.AddAuthentication()
            .AddGoogle(fun o -> 
                        o.ClientId <- "665405153593-0aqh263tcs11et51ki8n9tff1ni1u0i9.apps.googleusercontent.com"
                        o.ClientSecret <- "") |> ignore
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "wwwroot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0