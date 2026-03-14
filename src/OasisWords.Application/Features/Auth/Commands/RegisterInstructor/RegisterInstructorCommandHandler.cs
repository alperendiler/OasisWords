using MediatR;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.UserService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.RegisterInstructor;

public class RegisterInstructorCommandHandler : IRequestHandler<RegisterInstructorCommand, RegisterInstructorResponse>
{
    private readonly IUserRepository              _userRepository;
    private readonly IRefreshTokenRepository      _refreshTokenRepository;
    private readonly IUserOperationClaimRepository _userClaimRepository;
    private readonly IOperationClaimRepository    _operationClaimRepository;
    private readonly ITokenHelper                 _tokenHelper;
    private readonly AuthBusinessRules            _rules;

    public RegisterInstructorCommandHandler(
        IUserRepository               userRepository,
        IRefreshTokenRepository       refreshTokenRepository,
        IUserOperationClaimRepository  userClaimRepository,
        IOperationClaimRepository     operationClaimRepository,
        ITokenHelper                  tokenHelper,
        AuthBusinessRules             rules)
    {
        _userRepository           = userRepository;
        _refreshTokenRepository   = refreshTokenRepository;
        _userClaimRepository      = userClaimRepository;
        _operationClaimRepository = operationClaimRepository;
        _tokenHelper              = tokenHelper;
        _rules                    = rules;
    }

    public async Task<RegisterInstructorResponse> Handle(RegisterInstructorCommand request, CancellationToken ct)
    {
        await _rules.EmailCannotBeDuplicatedWhenRegistered(request.Email, ct);

        // 1 — Kullanıcı oluştur
        HashingHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        User user = new()
        {
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
        user = await _userRepository.AddAsync(user, ct);

        // 2 — "Instructor" operation claim'ini bul ve ata
        OperationClaim? instructorClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == "Instructor", cancellationToken: ct);

        if (instructorClaim is not null)
        {
            await _userClaimRepository.AddAsync(new UserOperationClaim
            {
                UserId           = user.Id,
                OperationClaimId = instructorClaim.Id,
                CreatedAt        = DateTime.UtcNow
            }, ct);
        }

        // 3 — Token üret
        IList<OperationClaim> claims = instructorClaim is not null
            ? new List<OperationClaim> { instructorClaim }
            : new List<OperationClaim>();

        AccessToken  accessToken  = _tokenHelper.CreateToken(user, claims);
        RefreshToken refreshToken = _tokenHelper.CreateRefreshToken(user, string.Empty);
        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return new RegisterInstructorResponse
        {
            UserId       = user.Id,
            AccessToken  = accessToken,
            RefreshToken = refreshToken.Token
        };
    }
}
