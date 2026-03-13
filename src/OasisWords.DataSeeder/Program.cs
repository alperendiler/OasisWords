using OasisWords.DataSeeder;
using OasisWords.DataSeeder.Models;
using OasisWords.DataSeeder.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console())
    .ConfigureServices((ctx, services) =>
    {
        SeederSettings settings = ctx.Configuration
            .GetSection("SeederSettings")
            .Get<SeederSettings>() ?? new SeederSettings();

        services.AddSingleton(settings);
        services.AddHttpClient<GeminiTranslationService>();
        services.AddHttpClient<OasisWordsApiClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }); services.AddHostedService<SeederWorker>();
    })
    .Build();

await host.RunAsync();
