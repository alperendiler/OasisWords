namespace OasisWords.Application.Features.Words.Commands.DeleteWordMeaning;

public class DeleteWordMeaningCommand : MediatR.IRequest<DeleteWordMeaningResponse>
{
    public Guid Id { get; set; }
}
