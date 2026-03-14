using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenResponse
{
    public AccessToken AccessToken  { get; set; } = null!;
    public string      RefreshToken { get; set; } = string.Empty;
}
