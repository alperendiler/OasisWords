using FluentValidation;
using OasisWords.Application.Features.Words.Constants;

namespace OasisWords.Application.Features.Words.Commands.DeleteWord;

public class DeleteWordCommandValidator : AbstractValidator<DeleteWordCommand>
{
    public DeleteWordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(WordMessages.WordIdRequired);
    }
}
