namespace OasisWords.Application.Features.Languages.Commands.DeleteLanguage;

public class DeleteLanguageCommand : MediatR.IRequest<DeleteLanguageResponse>
{
    public Guid Id { get; set; }
}
