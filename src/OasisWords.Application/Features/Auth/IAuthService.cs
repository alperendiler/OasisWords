using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth;

public interface IAuthService
{

    Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken> UseRefreshTokenAsync(string token, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason, CancellationToken cancellationToken = default);
}
