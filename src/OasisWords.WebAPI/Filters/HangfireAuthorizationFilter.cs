using Hangfire.Dashboard;
using System.Security.Claims;

namespace OasisWords.WebAPI.Filters;

/// <summary>
/// Guards the Hangfire dashboard (/hangfire) against unauthorised access.
///
/// Access is granted only when ALL of the following conditions are true:
///   1. The HTTP request carries a valid, non-expired JWT (handled upstream by
///      <c>UseAuthentication()</c> — the identity is already populated by the
///      time this filter runs).
///   2. The authenticated user has the "Admin" role claim.
///
/// In Development the filter is bypassed entirely (see Program.cs) so that
/// developers can access the dashboard without needing a token in the browser.
///
/// Usage in Program.cs:
/// <code>
/// app.UseHangfireDashboard("/hangfire", new DashboardOptions
/// {
///     Authorization = new[] { new HangfireAuthorizationFilter() }
/// });
/// </code>
/// </summary>
public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const string RequiredRole = "Admin";

    /// <inheritdoc />
    public bool Authorize(DashboardContext context)
    {
        HttpContext httpContext = context.GetHttpContext();

        // Must be authenticated
        if (httpContext.User.Identity is null || !httpContext.User.Identity.IsAuthenticated)
            return false;

        // Must carry the Admin role claim
        return httpContext.User.IsInRole(RequiredRole)
            || httpContext.User.HasClaim(ClaimTypes.Role, RequiredRole);
    }
}
