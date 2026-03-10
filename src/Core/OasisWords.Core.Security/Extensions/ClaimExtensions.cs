using System.Security.Claims;

namespace OasisWords.Core.Security.Extensions;

public static class ClaimExtensions
{
    public static void AddEmail(this ICollection<Claim> claims, string email)
        => claims.Add(new Claim(ClaimTypes.Email, email));

    public static void AddName(this ICollection<Claim> claims, string name)
        => claims.Add(new Claim(ClaimTypes.Name, name));

    public static void AddNameIdentifier(this ICollection<Claim> claims, string nameIdentifier)
        => claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));

    public static void AddRoles(this ICollection<Claim> claims, string[] roles)
        => roles.ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));
}

public static class ClaimsPrincipalExtensions
{
    public static string? GetEmail(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindFirstValue(ClaimTypes.Email);

    public static string? GetId(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        => Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static string[] GetRoles(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
}
