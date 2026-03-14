namespace OasisWords.Application.Features.Languages.Commands.CreateLanguage;

public class CreateLanguageCommand : MediatR.IRequest<CreateLanguageResponse>
{
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }
}
