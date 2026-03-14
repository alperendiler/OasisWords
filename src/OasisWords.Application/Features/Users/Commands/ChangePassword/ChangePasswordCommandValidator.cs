using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(AuthMessages.CurrentPasswordRequired);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(AuthMessages.NewPasswordRequired)
            .MinimumLength(8).WithMessage(AuthMessages.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(AuthMessages.PasswordMustHaveUpper)
            .Matches("[0-9]").WithMessage(AuthMessages.PasswordMustHaveDigit)
            .Matches("[^a-zA-Z0-9]").WithMessage(AuthMessages.PasswordMustHaveSpecial);
    }
}
