using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(AuthMessages.RefreshTokenRequired);
    }
}
