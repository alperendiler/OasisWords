using FluentValidation;
using OasisWords.Application.Features.Languages.Constants;

namespace OasisWords.Application.Features.Languages.Commands.UpdateLanguage;

public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    public UpdateLanguageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage(LanguageMessages.NameRequired)
                            .MaximumLength(100).WithMessage(LanguageMessages.NameTooLong);
        RuleFor(x => x.FlagImageUrl).MaximumLength(512).WithMessage(LanguageMessages.FlagUrlTooLong)
                                    .When(x => x.FlagImageUrl is not null);
    }
}
