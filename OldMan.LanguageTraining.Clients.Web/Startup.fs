namespace OldMan.LanguageTraining.Clients.WebHosting

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection

[<WebSharper.JavaScript(false)>]
type Startup() =
    member this.ConfigureServices(services: IServiceCollection) = ()

    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .Run(fun context ->
                context.Response.StatusCode <- 404
                context.Response.WriteAsync("Page not found"))
