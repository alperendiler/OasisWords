using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.Login;

public class LoginResponse
{
    public AccessToken? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public bool RequiresTwoFactor { get; set; }
}
