using Microsoft.IdentityModel.Tokens;
using OasisWords.Core.Security.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OasisWords.Core.Security.JWT;

public class TokenOptions
{
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int AccessTokenExpiration { get; set; } = 60; // minutes
    public string SecurityKey { get; set; } = string.Empty;
    public int RefreshTokenTTL { get; set; } = 7; // days
}

public class AccessToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}

public interface ITokenHelper
{
    AccessToken CreateToken(User user, IList<OperationClaim> operationClaims);
    RefreshToken CreateRefreshToken(User user, string ipAddress);
}

public class JwtHelper : ITokenHelper
{
    private readonly TokenOptions _tokenOptions;
    private DateTime _accessTokenExpiration;

    public JwtHelper(TokenOptions tokenOptions)
    {
        _tokenOptions = tokenOptions;
    }

    public AccessToken CreateToken(User user, IList<OperationClaim> operationClaims)
    {
        _accessTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration);
        SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha512Signature);

        JwtSecurityToken jwt = CreateJwtSecurityToken(user, signingCredentials, operationClaims);
        JwtSecurityTokenHandler tokenHandler = new();
        string token = tokenHandler.WriteToken(jwt);

        return new AccessToken { Token = token, Expiration = _accessTokenExpiration };
    }

    public RefreshToken CreateRefreshToken(User user, string ipAddress)
    {
        return new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenTTL),
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }

    private JwtSecurityToken CreateJwtSecurityToken(
        User user,
        SigningCredentials signingCredentials,
        IList<OperationClaim> operationClaims)
    {
        return new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: _accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: SetClaims(user, operationClaims),
            signingCredentials: signingCredentials);
    }

    private static IEnumerable<Claim> SetClaims(User user, IList<OperationClaim> operationClaims)
    {
        List<Claim> claims = new();
        claims.AddRange(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.AuthenticationMethod, user.AuthenticatorType.ToString())
        });

        claims.AddRange(operationClaims.Select(c => new Claim(ClaimTypes.Role, c.Name)));
        return claims;
    }
}
