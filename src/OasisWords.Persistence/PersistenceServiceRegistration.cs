using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OasisWords.Persistence.Contexts;

namespace OasisWords.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OasisWordsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("OasisWordsDB"),
                npgsql => npgsql.MigrationsAssembly(typeof(OasisWordsDbContext).Assembly.GetName().Name)));

        // Register repositories here
        // services.AddScoped<IWordRepository, WordRepository>();

        return services;
    }
}
