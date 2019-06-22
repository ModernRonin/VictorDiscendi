namespace OldMan.LanguageTraining.Clients.WebHosting

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting

[<WebSharper.JavaScript(false)>]
module Program =
    let BuildWebHost args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()

    [<EntryPoint>]
    let main args =
        BuildWebHost(args).Run()
        0