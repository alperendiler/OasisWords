using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Infrastructure.Adapters.Ai;

namespace OasisWords.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        GeminiSettings geminiSettings = configuration
            .GetSection("GeminiSettings")
            .Get<GeminiSettings>() ?? new GeminiSettings();

        services.AddSingleton(geminiSettings);

        services.AddHttpClient<GeminiAdapter>();
        services.AddScoped<IAiService, GeminiAdapter>();

        return services;
    }
}
