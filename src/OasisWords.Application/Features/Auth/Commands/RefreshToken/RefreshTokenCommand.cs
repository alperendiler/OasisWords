namespace OasisWords.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : MediatR.IRequest<RefreshTokenResponse>
{
    public string Token     { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}
