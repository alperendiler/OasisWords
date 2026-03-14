using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.DeleteWordMeaning;

public class DeleteWordMeaningCommandValidator : AbstractValidator<DeleteWordMeaningCommand>
{
    public DeleteWordMeaningCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(WordMessages.MeaningNotFound);
    }
}
