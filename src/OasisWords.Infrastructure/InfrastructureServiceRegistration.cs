using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OasisWords.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register HttpClients for external services
        // services.AddHttpClient<OpenAiAdapter>();

        // Register external service adapters
        // services.AddScoped<IAiService, OpenAiAdapter>();
        // services.AddScoped<ITranslationService, DeepLAdapter>();

        return services;
    }
}
