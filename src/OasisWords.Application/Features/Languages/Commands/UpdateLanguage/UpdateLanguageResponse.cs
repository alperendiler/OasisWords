namespace OasisWords.Application.Features.Languages.Commands.UpdateLanguage;

public class UpdateLanguageResponse
{
    public Guid   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
