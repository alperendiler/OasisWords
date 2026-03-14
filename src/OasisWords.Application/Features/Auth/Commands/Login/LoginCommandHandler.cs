using MediatR;
using OasisWords.Application.Features.Auth.Commands.Register;
using OasisWords.Application.Features.Auth.Rules;
using OasisWords.Application.Services.AuthService;
using OasisWords.Core.Mailing.Models;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthService _authService;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly IUserSer

    public LoginCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        await _rules.UserShouldExistWhenRequested(email, ct);

        User user = await _userRepository.GetAsync(u => u.Email == email, cancellationToken: ct)
            ?? throw new InvalidOperationException();

        _rules.UserShouldBeActive(user);

        if (!HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            throw new OasisWords.Core.CrossCuttingConcerns.Exceptions.BusinessException(
                Features.Auth.Constants.AuthMessages.InvalidCredentials);

        if (user.AuthenticatorType != AuthenticatorType.None)
            return new LoginResponse { RequiresTwoFactor = true };

        AccessToken access = await CreateAccessTokenAsync(user, ct);
        RefreshToken refresh = _tokenHelper.CreateRefreshToken(user, ipAddress);
        await _refreshTokenRepository.AddAsync(refresh, ct);

        return new LoginResponse { AccessToken = access, RefreshToken = refresh.Token };
    }
}

    // ?? Temel kayýt (GenericRegister — RegisterStudentCommand bunu çaðýrýr) ??
    public async Task<RegisterResponse> RegisterAsync(User user, CancellationToken ct = default)
    {
        await _rules.EmailCannotBeDuplicatedWhenRegistered(user.Email, ct);
        User created = await _userRepository.AddAsync(user, ct);

        AccessToken access = await CreateAccessTokenAsync(created, ct);
        RefreshToken refresh = _tokenHelper.CreateRefreshToken(created, string.Empty);
        await _refreshTokenRepository.AddAsync(refresh, ct);

        return new RegisterResponse { AccessToken = access, RefreshToken = refresh };
    }

    // ?? Login ?????????????????????????????????????????????????????????????????
    public async Task<LoginResponse> LoginAsync(
        string email, string password, string ipAddress, CancellationToken ct = default)
    {
      
    }