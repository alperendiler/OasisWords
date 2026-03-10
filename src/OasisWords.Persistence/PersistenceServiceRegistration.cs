using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Application.Services.AuthService;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.WordService;
using OasisWords.Persistence.Contexts;
using OasisWords.Persistence.Repositories;

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

        // Auth repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOperationClaimRepository, OperationClaimRepository>();
        services.AddScoped<IUserOperationClaimRepository, UserOperationClaimRepository>();

        // Word repositories
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IWordMeaningRepository, WordMeaningRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();

        // Student progress repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentWordProgressRepository, StudentWordProgressRepository>();
        services.AddScoped<IStudentStreakRepository, StudentStreakRepository>();
        services.AddScoped<IDailyTargetSessionRepository, DailyTargetSessionRepository>();

        // AI dialogue repositories
        services.AddScoped<IAiDialogueSessionRepository, AiDialogueSessionRepository>();
        services.AddScoped<IAiDialogueMessageRepository, AiDialogueMessageRepository>();
        services.AddScoped<IAiDialogueTargetWordRepository, AiDialogueTargetWordRepository>();

        return services;
    }
}
