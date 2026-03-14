namespace OasisWords.Application.Features.Languages.Commands.UpdateLanguage;

public class UpdateLanguageCommand : MediatR.IRequest<UpdateLanguageResponse>
{
    public Guid    Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }
}
