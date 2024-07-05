namespace QueryCV
#nowarn "20"
open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()

        let app = builder.Build()

        app.UseHttpsRedirection()
        let fb (b: CorsPolicyBuilder) =
            b.WithOrigins([|"http://localhost:3000"|]).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
            ()
        app.UseCors(Action<CorsPolicyBuilder> fb)

        app.UseAuthorization()
        app.MapControllers()
        
        // app.UseSwagger()

        app.Run()

        exitCode