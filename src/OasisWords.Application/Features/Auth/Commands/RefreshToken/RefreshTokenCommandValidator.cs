using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(AuthMessages.RefreshTokenRequired);
    }
}
