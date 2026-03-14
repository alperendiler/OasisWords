namespace OasisWords.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "Token has been revoked.";
}
