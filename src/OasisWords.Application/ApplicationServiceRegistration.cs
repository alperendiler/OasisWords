using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OasisWords.Application.Features.Auth;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.AuthService;
using OasisWords.Core.Application.Pipelines;
using System.Reflection;

namespace OasisWords.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Pipeline behaviors — order matters
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));

        // Application services
        services.AddScoped<IAuthService, AuthManager>();

        // Business rules
        services.AddScoped<AuthBusinessRules>();
        services.AddScoped<WordBusinessRules>();

        return services;
    }
}
