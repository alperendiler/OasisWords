using MediatR;
using OasisWords.Application.Services.AuthService;

namespace OasisWords.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, RevokeTokenResponse>
{
    private readonly IAuthService _authService;

    public RevokeTokenCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<RevokeTokenResponse> Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        await _authService.RevokeRefreshTokenAsync(request.Token, request.IpAddress, request.Reason, ct);
        return new RevokeTokenResponse { Success = true };
    }
}
