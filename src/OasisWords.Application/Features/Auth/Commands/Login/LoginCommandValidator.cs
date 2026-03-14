using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(AuthMessages.EmailRequired)
                             .EmailAddress().WithMessage(AuthMessages.EmailInvalid);
        RuleFor(x => x.Password).NotEmpty().WithMessage(AuthMessages.PasswordRequired);
    }
}
