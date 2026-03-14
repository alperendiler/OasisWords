using MediatR;
using OasisWords.Application.Features.Auth.Constants;
using OasisWords.Application.Features.Users.Constants;
using OasisWords.Application.Services.UserService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;

namespace OasisWords.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IUserRepository _userRepo;

    public ChangePasswordCommandHandler(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        User user = await _userRepo.GetAsync(u => u.Id == request.UserId, cancellationToken: ct)
            ?? throw new NotFoundException(string.Format(UserMessages.UserNotFound, request.UserId));

        // Mevcut şifreyi doğrula
        if (!HashingHelper.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw new BusinessException(AuthMessages.CurrentPasswordWrong);

        // Yeni şifre eskiyle aynı olmamalı
        if (HashingHelper.VerifyPasswordHash(request.NewPassword, user.PasswordHash, user.PasswordSalt))
            throw new BusinessException(AuthMessages.NewPasswordSameAsOld);

        // Yeni hash üret ve kaydet
        HashingHelper.CreatePasswordHash(request.NewPassword, out byte[] newHash, out byte[] newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;
        await _userRepo.UpdateAsync(user, ct);

        return new ChangePasswordResponse { Success = true };
    }
}
