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

    public async Task EmailCannotBeDuplicatedWhenRegistered(string email, CancellationToken cancellationToken = default)
    {
        bool exists = await _userRepository.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
            throw new BusinessException("A user with this email address already exists.");
    }

    public void UserShouldBeActive(User user)
    {
        if (!user.IsActive)
            throw new BusinessException("This account has been deactivated.");
    }

    public void UserPasswordShouldMatch(User user, byte[] passwordHash)
    {
        if (!user.PasswordHash.SequenceEqual(passwordHash))
            throw new BusinessException("Email or password is incorrect.");
    }

    public async Task UserShouldExistWhenRequested(string email, CancellationToken cancellationToken = default)
    {
        bool exists = await _userRepository.AnyAsync(u => u.Email == email, cancellationToken);
        if (!exists)
            throw new BusinessException("Email or password is incorrect.");
    }

    public void RefreshTokenShouldBeActive(RefreshToken refreshToken)
    {
        if (!refreshToken.IsActive)
            throw new BusinessException("Refresh token is no longer active.");
    }
}
