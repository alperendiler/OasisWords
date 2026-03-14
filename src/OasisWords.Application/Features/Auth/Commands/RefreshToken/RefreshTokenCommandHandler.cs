using MediatR;
using OasisWords.Application.Services.AuthService;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        Core.Security.Entities.RefreshToken newToken =
            await _authService.UseRefreshTokenAsync(request.Token, request.IpAddress, ct);

        // Yeni refresh token ile yeni bir access token üretmek için kullanıcıyı alıyoruz.
        // AuthManager.UseRefreshTokenAsync zaten yeni token döndürür —
        // burada sadece yapıyı response'a dönüştürüyoruz.
        return new RefreshTokenResponse
        {
            // AccessToken, AuthManager'ın UseRefreshTokenAsync'inden gelmiyor;
            // çağrı zincirini tamamlamak için ayrı bir adım gerekiyor.
            // Bu nedenle IAuthService'e CreateAccessTokenForRefreshAsync eklenmiştir.
            AccessToken  = await _authService.CreateAccessTokenForRefreshAsync(newToken, ct),
            RefreshToken = newToken.Token
        };
    }
}
