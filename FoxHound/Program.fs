namespace FoxHound.Web

open System
open System.IO
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Serilog

module Program =
    let exitCode = 0
    
    [<AbstractClass; Sealed; Extension>]
    type Extensions private () =
        [<Extension>]
        static member AddLocalAppSettings(builder: IConfigurationBuilder) =
            builder.AddJsonFile("appsettings.Local.json", true, true)
    
    let GetCurrentConfiguration () =
        let environment = match Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") with
                          | null -> "Production"
                          | s -> s
      
        ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(String.Format("appsettings.{0}.json", environment), false, true)
            .AddLocalAppSettings()
            .AddEnvironmentVariables()
            
    let CreateLogger(currentConfiguation) =
        LoggerConfiguration().ReadFrom.Configuration(currentConfiguation).CreateLogger()

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.ConfigureAppConfiguration(fun config -> config.AddLocalAppSettings() |> ignore) |> ignore
                webBuilder.UseStartup<Startup>() |> ignore
            )
            .UseSerilog()

    [<EntryPoint>]
    let main args =
        let currentConfiguration = GetCurrentConfiguration().Build()
        Log.Logger <- CreateLogger(currentConfiguration)
        
        try
            try
                Log.Information("Starting FoxHound")
                CreateHostBuilder(args).Build().Run()
                Log.Information("FoxHound has stopped")
            with
            | :? Exception -> Log.Error("Failed to start FoxHound")
        finally
            Log.CloseAndFlush()
            
        exitCode
