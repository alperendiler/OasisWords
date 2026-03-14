using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.UpdateWordMeaning;

public class UpdateWordMeaningCommandValidator : AbstractValidator<UpdateWordMeaningCommand>
{
    public UpdateWordMeaningCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(WordMessages.WordIdRequired);
        RuleFor(x => x.TranslationText).NotEmpty().WithMessage(WordMessages.MeaningRequired)
                                       .MaximumLength(500).WithMessage(WordMessages.MeaningTooLong);
        RuleFor(x => x.ExampleSentence).MaximumLength(1000).WithMessage(WordMessages.ExampleSentenceTooLong)
                                       .When(x => x.ExampleSentence is not null);
        RuleFor(x => x.ExampleTranslation).MaximumLength(1000).WithMessage(WordMessages.ExampleTranslationTooLong)
                                          .When(x => x.ExampleTranslation is not null);
    }
}
