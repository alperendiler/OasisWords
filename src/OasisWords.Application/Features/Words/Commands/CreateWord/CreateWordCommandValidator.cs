using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.CreateWord;

public class CreateWordCommandValidator : AbstractValidator<CreateWordCommand>
{
    public CreateWordCommandValidator()
    {
        RuleFor(x => x.LanguageId).NotEmpty().WithMessage(WordMessages.LanguageRequired);
        RuleFor(x => x.Text).NotEmpty().WithMessage(WordMessages.WordRequired)
                            .MaximumLength(200).WithMessage(WordMessages.WordTooLong);
        RuleFor(x => x.PhoneticSpelling).MaximumLength(200).WithMessage(WordMessages.PhoneticTooLong)
                                        .When(x => x.PhoneticSpelling is not null);
        RuleFor(x => x.PronunciationAudioUrl).MaximumLength(512).WithMessage(WordMessages.AudioUrlTooLong)
                                             .When(x => x.PronunciationAudioUrl is not null);
    }
}
