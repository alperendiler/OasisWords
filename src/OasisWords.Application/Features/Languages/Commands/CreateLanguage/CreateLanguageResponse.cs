namespace OasisWords.Application.Features.Languages.Commands.CreateLanguage;

public class CreateLanguageResponse
{
    public Guid   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
