namespace OldMan.LanguageTraining.Clients.Web

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting

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