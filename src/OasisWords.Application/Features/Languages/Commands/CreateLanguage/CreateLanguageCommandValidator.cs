using FluentValidation;
using OasisWords.Application.Features.Languages.Constants;

namespace OasisWords.Application.Features.Languages.Commands.CreateLanguage;

public class CreateLanguageCommandValidator : AbstractValidator<CreateLanguageCommand>
{
    public CreateLanguageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(LanguageMessages.NameRequired)
                            .MaximumLength(100).WithMessage(LanguageMessages.NameTooLong);
        RuleFor(x => x.Code).NotEmpty().WithMessage(LanguageMessages.CodeRequired)
                            .MaximumLength(10).WithMessage(LanguageMessages.CodeTooLong);
        RuleFor(x => x.FlagImageUrl).MaximumLength(512).WithMessage(LanguageMessages.FlagUrlTooLong)
                                    .When(x => x.FlagImageUrl is not null);
    }
}
