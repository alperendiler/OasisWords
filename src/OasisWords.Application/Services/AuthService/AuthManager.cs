using Microsoft.EntityFrameworkCore;
using OasisWords.Application.Features.Auth;
using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Services.AuthService;

public class AuthManager : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository; 
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly AuthBusinessRules _authBusinessRules;

    public AuthManager(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        ITokenHelper tokenHelper,
        AuthBusinessRules authBusinessRules)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _tokenHelper = tokenHelper;
        _authBusinessRules = authBusinessRules;
    }

   

    public async Task<LoginResponse> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _authBusinessRules.UserShouldExistWhenRequested(email, cancellationToken);

        User user = await _userRepository.GetAsync(u => u.Email == email, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException();

        _authBusinessRules.UserShouldBeActive(user);

        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        if (!HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            _authBusinessRules.UserPasswordShouldMatch(user, hash);
        }

        if (user.AuthenticatorType != AuthenticatorType.None)
        {
            return new LoginResponse { RequiresTwoFactor = true };
        }

        AccessToken accessToken = await CreateAccessTokenAsync(user, cancellationToken);
        RefreshToken refreshToken = _tokenHelper.CreateRefreshToken(user, ipAddress);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            RequiresTwoFactor = false
        };
    }

    public async Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        IList<OperationClaim> operationClaims = await GetUserOperationClaimsAsync(user.Id, cancellationToken);
        return _tokenHelper.CreateToken(user, operationClaims);
    }

    public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        return await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken> UseRefreshTokenAsync(
        string token,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await _refreshTokenRepository.GetAsync(
            rt => rt.Token == token,
            cancellationToken: cancellationToken);

        if (refreshToken is null)
            throw new InvalidOperationException("Refresh token not found.");

        _authBusinessRules.RefreshTokenShouldBeActive(refreshToken);

        User user = await _userRepository.GetAsync(
            u => u.Id == refreshToken.UserId,
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException();

        RefreshToken newRefreshToken = _tokenHelper.CreateRefreshToken(user, ipAddress);
        newRefreshToken = await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReplacedByToken = newRefreshToken.Token;
        refreshToken.ReasonRevoked = "Replaced by new token";
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        return newRefreshToken;
    }

    public async Task RevokeRefreshTokenAsync(
        string token,
        string ipAddress,
        string reason,
        CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await _refreshTokenRepository.GetAsync(
            rt => rt.Token == token,
            cancellationToken: cancellationToken);

        if (refreshToken is null)
            throw new InvalidOperationException("Refresh token not found.");

        _authBusinessRules.RefreshTokenShouldBeActive(refreshToken);

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = reason;
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
    }

    private async Task<IList<OperationClaim>> GetUserOperationClaimsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userClaims = await _userOperationClaimRepository.GetListAsync(
            predicate: uoc => uoc.UserId == userId,
            include: q => q.Include(uoc => uoc.OperationClaim),
            enableTracking: false,
            cancellationToken: cancellationToken);

        return userClaims.Items.Select(uoc => uoc.OperationClaim).ToList();
    }

   
}
