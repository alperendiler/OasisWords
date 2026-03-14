using OasisWords.Application.Features.Auth;
using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.UserService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Services.AuthService;

/// <summary>
/// Sadeleştirilmiş AuthManager — yalnızca token üretimi, şifre doğrulama ve
/// refresh token yönetimi burada. Kayıt iş mantığı ilgili Command Handler'lara taşındı.
/// </summary>
public class AuthManager : IAuthService
{
    private readonly IUserRepository              _userRepository;
    private readonly IRefreshTokenRepository      _refreshTokenRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly ITokenHelper                 _tokenHelper;
    private readonly AuthBusinessRules            _rules;

    public AuthManager(
        IUserRepository               userRepository,
        IRefreshTokenRepository       refreshTokenRepository,
        IUserOperationClaimRepository  userOperationClaimRepository,
        ITokenHelper                  tokenHelper,
        AuthBusinessRules             rules)
    {
        _userRepository               = userRepository;
        _refreshTokenRepository       = refreshTokenRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _tokenHelper                  = tokenHelper;
        _rules                        = rules;
    }


    // ── Token üretimi ─────────────────────────────────────────────────────────
    public async Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken ct = default)
    {
        IList<OperationClaim> claims = await GetClaimsAsync(user.Id, ct);
        return _tokenHelper.CreateToken(user, claims);
    }

    public async Task<AccessToken> CreateAccessTokenForRefreshAsync(
        RefreshToken refreshToken, CancellationToken ct = default)
    {
        User user = await _userRepository.GetAsync(u => u.Id == refreshToken.UserId, cancellationToken: ct)
            ?? throw new InvalidOperationException();
        return await CreateAccessTokenAsync(user, ct);
    }

    // ── Refresh token işlemleri ───────────────────────────────────────────────
    public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
        => await _refreshTokenRepository.AddAsync(refreshToken, ct);

    public async Task<RefreshToken> UseRefreshTokenAsync(
        string token, string ipAddress, CancellationToken ct = default)
    {
        RefreshToken? existing = await _refreshTokenRepository.GetAsync(
            rt => rt.Token == token, cancellationToken: ct);

        if (existing is null)
            throw new InvalidOperationException(Features.Auth.Constants.AuthMessages.RefreshTokenNotFound);

        _rules.RefreshTokenShouldBeActive(existing);

        User user = await _userRepository.GetAsync(u => u.Id == existing.UserId, cancellationToken: ct)
            ?? throw new InvalidOperationException();

        RefreshToken newToken = _tokenHelper.CreateRefreshToken(user, ipAddress);
        newToken = await _refreshTokenRepository.AddAsync(newToken, ct);

        existing.Revoked         = DateTime.UtcNow;
        existing.RevokedByIp     = ipAddress;
        existing.ReplacedByToken = newToken.Token;
        existing.ReasonRevoked   = "Replaced by new token";
        await _refreshTokenRepository.UpdateAsync(existing, ct);

        return newToken;
    }

    public async Task RevokeRefreshTokenAsync(
        string token, string ipAddress, string reason, CancellationToken ct = default)
    {
        RefreshToken? existing = await _refreshTokenRepository.GetAsync(
            rt => rt.Token == token, cancellationToken: ct);

        if (existing is null)
            throw new InvalidOperationException(Features.Auth.Constants.AuthMessages.RefreshTokenNotFound);

        _rules.RefreshTokenShouldBeActive(existing);

        existing.Revoked     = DateTime.UtcNow;
        existing.RevokedByIp = ipAddress;
        existing.ReasonRevoked = reason;
        await _refreshTokenRepository.UpdateAsync(existing, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────
    private async Task<IList<OperationClaim>> GetClaimsAsync(Guid userId, CancellationToken ct)
    {
        var userClaims = await _userOperationClaimRepository.GetListAsync(
            predicate: uoc => uoc.UserId == userId,
            include: q => q.Include(uoc => uoc.OperationClaim),
            enableTracking: false,
            cancellationToken: ct);

        return userClaims.Items.Select(uoc => uoc.OperationClaim).ToList();
    }
}
