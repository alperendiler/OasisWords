namespace OasisWords.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommand : MediatR.IRequest<RevokeTokenResponse>
{
    public string Token     { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Reason    { get; set; } = "Revoked by user";
}
