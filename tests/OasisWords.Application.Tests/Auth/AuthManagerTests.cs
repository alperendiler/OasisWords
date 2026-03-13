using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OasisWords.Application.Features.Auth;
using OasisWords.Application.Features.Auth.Commands.Login;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.AuthService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;
using System.Linq.Expressions;
using Xunit;

namespace OasisWords.Application.Tests.Auth;

public class AuthManagerTests
{
    // ── Fixture setup ─────────────────────────────────────────────────────────

    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();
    private readonly Mock<IUserOperationClaimRepository> _claimRepoMock = new();
    private readonly Mock<ITokenHelper> _tokenHelperMock = new();
    private readonly Mock<AuthBusinessRules> _rulesMock;
    private readonly AuthManager _sut;

    public AuthManagerTests()
    {
        // AuthBusinessRules depends on IAsyncRepository<User,Guid> — pass the mock's object
        _rulesMock = new Mock<AuthBusinessRules>(_userRepoMock.Object);

        _sut = new AuthManager(
            _userRepoMock.Object,
            _refreshRepoMock.Object,
            _claimRepoMock.Object,
            _tokenHelperMock.Object,
            _rulesMock.Object);
    }

    // ── Register tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidUser_ReturnsAccessAndRefreshToken()
    {
        // Arrange
        User user = BuildUser();
        AccessToken expectedAccess = new() { Token = "jwt.token.here", Expiration = DateTime.UtcNow.AddHours(1) };
        RefreshToken expectedRefresh = new() { Token = "refresh-abc", UserId = user.Id };

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupEmptyOperationClaims(user.Id);
        _tokenHelperMock.Setup(t => t.CreateToken(user, It.IsAny<IList<OperationClaim>>()))
            .Returns(expectedAccess);
        _tokenHelperMock.Setup(t => t.CreateRefreshToken(user, It.IsAny<string>()))
            .Returns(expectedRefresh);
        _refreshRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRefresh);

        // Act
        RegisterResponse result = await _sut.RegisterAsync(user);

        // Assert
        result.AccessToken.Should().NotBeNull();
        result.AccessToken.Token.Should().Be("jwt.token.here");
        result.RefreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsBusinessException()
    {
        // The business rule mock throws when email already exists
        _rulesMock
            .Setup(r => r.EmailCannotBeDuplicatedWhenRegistered(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException("A user with this email address already exists."));

        User user = BuildUser();
        Func<Task> act = async () => await _sut.RegisterAsync(user);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*email*");
    }

    // ── Login tests ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsTokens()
    {
        const string password = "MyStrongPass@1";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        User user = BuildUser(hash, salt);
        AccessToken access = new() { Token = "access-token" };
        RefreshToken refresh = new() { Token = "refresh-token", UserId = user.Id };

        _userRepoMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>,
                    Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<User, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        SetupEmptyOperationClaims(user.Id);
        _tokenHelperMock.Setup(t => t.CreateToken(user, It.IsAny<IList<OperationClaim>>())).Returns(access);
        _tokenHelperMock.Setup(t => t.CreateRefreshToken(user, It.IsAny<string>())).Returns(refresh);
        _refreshRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refresh);

        LoginResponse result = await _sut.LoginAsync(user.Email, password, "127.0.0.1");

        result.AccessToken.Should().NotBeNull();
        result.AccessToken!.Token.Should().Be("access-token");
        result.RequiresTwoFactor.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_RequiresTwoFactor_WhenAuthenticatorTypeIsNotNone()
    {
        const string password = "Pass@2024";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        User user = BuildUser(hash, salt);
        user.AuthenticatorType = AuthenticatorType.Otp;

        _userRepoMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>,
                    Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<User, object>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        LoginResponse result = await _sut.LoginAsync(user.Email, password, "127.0.0.1");

        result.RequiresTwoFactor.Should().BeTrue();
        result.AccessToken.Should().BeNull();
    }

    // ── CreateAccessToken tests ───────────────────────────────────────────────

    [Fact]
    public async Task CreateAccessTokenAsync_LoadsClaimsAndCallsTokenHelper()
    {
        User user = BuildUser();
        OperationClaim claim = new() { Id = Guid.NewGuid(), Name = "Student" };

        SetupOperationClaims(user.Id, claim);
        _tokenHelperMock
            .Setup(t => t.CreateToken(user, It.Is<IList<OperationClaim>>(c => c.Any(x => x.Name == "Student"))))
            .Returns(new AccessToken { Token = "token-with-role" });

        AccessToken result = await _sut.CreateAccessTokenAsync(user);

        result.Token.Should().Be("token-with-role");
        _tokenHelperMock.Verify(
            t => t.CreateToken(user, It.Is<IList<OperationClaim>>(c => c.Count == 1)),
            Times.Once);
    }

    // ── Token helper (pure unit) ──────────────────────────────────────────────

    [Fact]
    public void JwtHelper_CreateToken_ProducesNonEmptyJwt()
    {
        TokenOptions opts = new()
        {
            Audience = "test-audience",
            Issuer = "test-issuer",
            SecurityKey = "SuperSecretKeyThatIsAtLeast32CharsLong!!",
            AccessTokenExpiration = 30,
            RefreshTokenTTL = 7
        };

        JwtHelper helper = new(opts);
        User user = BuildUser();

        AccessToken token = helper.CreateToken(user, new List<OperationClaim>
        {
            new() { Id = Guid.NewGuid(), Name = "Student" }
        });

        token.Token.Should().NotBeNullOrEmpty("JWT must be produced");
        token.Token.Split('.').Should().HaveCount(3, "JWT format is header.payload.signature");
        token.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void JwtHelper_CreateRefreshToken_ProducesUniqueTokens()
    {
        TokenOptions opts = new()
        {
            SecurityKey = "SuperSecretKeyThatIsAtLeast32CharsLong!!",
            RefreshTokenTTL = 7,
            Audience = "a",
            Issuer = "a"
        };
        JwtHelper helper = new(opts);
        User user = BuildUser();

        RefreshToken t1 = helper.CreateRefreshToken(user, "127.0.0.1");
        RefreshToken t2 = helper.CreateRefreshToken(user, "127.0.0.1");

        t1.Token.Should().NotBe(t2.Token, "each refresh token must be cryptographically unique");
        t1.Expires.Should().BeAfter(DateTime.UtcNow);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User BuildUser(byte[]? hash = null, byte[]? salt = null)
    {
        if (hash is null || salt is null)
            HashingHelper.CreatePasswordHash("DefaultPass@1", out hash, out salt);

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@oasiswords.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            AuthenticatorType = AuthenticatorType.None
        };
    }

    private void SetupEmptyOperationClaims(Guid userId)
        => SetupOperationClaims(userId);

    private void SetupOperationClaims(Guid userId, params OperationClaim[] claims)
    {
        var paginate = new Paginate<UserOperationClaim>
        {
            Items = claims.Select(c => new UserOperationClaim
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OperationClaimId = c.Id,
                OperationClaim = c
            }).ToList(),
            Index = 0, Size = 10, Count = claims.Length, Pages = 1
        };
        _claimRepoMock
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<UserOperationClaim, bool>>>(), // predicate
                It.IsAny<Func<IQueryable<UserOperationClaim>, IOrderedQueryable<UserOperationClaim>>>(), // orderBy
                It.IsAny<Func<IQueryable<UserOperationClaim>, IIncludableQueryable<UserOperationClaim, object>>>(), // include
                It.IsAny<int>(), // index
                It.IsAny<int>(), // size
                It.IsAny<bool>(), // enableTracking
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginate);
    }
}
