namespace OasisWords.Application.Features.Words.Commands.CreateWord;

public class CreateWordResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid LanguageId { get; set; }
}
