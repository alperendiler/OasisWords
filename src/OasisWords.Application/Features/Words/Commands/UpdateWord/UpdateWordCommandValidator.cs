using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.UpdateWord;

public class UpdateWordCommandValidator : AbstractValidator<UpdateWordCommand>
{
    public UpdateWordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(WordMessages.WordIdRequired);
        RuleFor(x => x.PhoneticSpelling).MaximumLength(200).WithMessage(WordMessages.PhoneticTooLong)
                                        .When(x => x.PhoneticSpelling is not null);
        RuleFor(x => x.PronunciationAudioUrl).MaximumLength(512).WithMessage(WordMessages.AudioUrlTooLong)
                                             .When(x => x.PronunciationAudioUrl is not null);
    }
}
