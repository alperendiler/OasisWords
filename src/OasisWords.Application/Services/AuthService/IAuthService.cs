using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Services.AuthService;

/// <summary>
/// Sadeleştirilmiş IAuthService — yalnızca token üretimi ve temel kimlik doğrulama operasyonları.
/// Kayıt işlemleri artık RegisterStudentCommand ve RegisterInstructorCommand Handler'larında.
/// </summary>
public interface IAuthService
{
   
    Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken ct = default);
    Task<AccessToken> CreateAccessTokenForRefreshAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<RefreshToken> UseRefreshTokenAsync(string token, string ipAddress, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason, CancellationToken ct = default);
}
