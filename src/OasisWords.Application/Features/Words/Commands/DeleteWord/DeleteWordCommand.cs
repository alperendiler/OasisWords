namespace OasisWords.Application.Features.Words.Commands.DeleteWord;

public class DeleteWordCommand : MediatR.IRequest<DeleteWordResponse>
{
    public Guid Id { get; set; }
}
