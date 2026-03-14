using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.CreateWordMeaning;

public class CreateWordMeaningCommandValidator : AbstractValidator<CreateWordMeaningCommand>
{
    public CreateWordMeaningCommandValidator()
    {
        RuleFor(x => x.WordId).NotEmpty().WithMessage(WordMessages.WordIdRequired);
        RuleFor(x => x.TranslationLanguageId).NotEmpty().WithMessage(WordMessages.TranslationLanguageRequired);
        RuleFor(x => x.CefrLevel).IsInEnum().WithMessage(WordMessages.CefrLevelInvalid);
        RuleFor(x => x.TranslationText).NotEmpty().WithMessage(WordMessages.MeaningRequired)
                                       .MaximumLength(500).WithMessage(WordMessages.MeaningTooLong);
        RuleFor(x => x.ExampleSentence).MaximumLength(1000).WithMessage(WordMessages.ExampleSentenceTooLong)
                                       .When(x => x.ExampleSentence is not null);
        RuleFor(x => x.ExampleTranslation).MaximumLength(1000).WithMessage(WordMessages.ExampleTranslationTooLong)
                                          .When(x => x.ExampleTranslation is not null);
    }
}
