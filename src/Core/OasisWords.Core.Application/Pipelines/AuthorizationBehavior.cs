using MediatR;
using Microsoft.Extensions.Logging;

namespace OasisWords.Core.Application.Pipelines;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();

        // Check for [Secured] attribute on the request class
        bool requiresAuthorization = requestType
            .GetCustomAttributes(typeof(SecuredOperationAttribute), true)
            .Length > 0;

        if (requiresAuthorization)
        {
            _logger.LogInformation("Authorization check for request: {RequestType}", requestType.Name);
            // TODO: Inject IHttpContextAccessor and validate user claims here
            // Example: var user = _httpContextAccessor.HttpContext?.User;
            // if (user?.Identity?.IsAuthenticated != true) throw new AuthorizationException();
        }

        return await next();
    }
}

/// <summary>
/// Marks a CQRS request as requiring an authenticated (and optionally role-checked) user.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SecuredOperationAttribute : Attribute
{
    public string[] Roles { get; }

    public SecuredOperationAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
