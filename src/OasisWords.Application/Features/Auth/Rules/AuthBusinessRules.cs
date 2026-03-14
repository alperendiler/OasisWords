using OasisWords.Application.Features.Auth.Constants;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Persistence.Repositories;
using OasisWords.Core.Security.Entities;

namespace OasisWords.Application.Features.Auth.Rules;

public class AuthBusinessRules
{
    private readonly IAsyncRepository<User, Guid> _userRepository;

    public AuthBusinessRules(IAsyncRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task EmailCannotBeDuplicatedWhenRegistered(string email, CancellationToken ct = default)
    {
        bool exists = await _userRepository.AnyAsync(u => u.Email == email, ct);
        if (exists)
            throw new BusinessException(AuthMessages.EmailAlreadyExists);
    }

    public void UserShouldBeActive(User user)
    {
        if (!user.IsActive)
            throw new BusinessException(AuthMessages.AccountDeactivated);
    }

    public void UserPasswordShouldMatch(User user, byte[] passwordHash)
    {
        if (!user.PasswordHash.SequenceEqual(passwordHash))
            throw new BusinessException(AuthMessages.InvalidCredentials);
    }

    public async Task UserShouldExistWhenRequested(string email, CancellationToken ct = default)
    {
        bool exists = await _userRepository.AnyAsync(u => u.Email == email, ct);
        if (!exists)
            throw new BusinessException(AuthMessages.UserNotFound);
    }

    public void RefreshTokenShouldBeActive(RefreshToken refreshToken)
    {
        if (!refreshToken.IsActive)
            throw new BusinessException(AuthMessages.RefreshTokenInactive);
    }
}
