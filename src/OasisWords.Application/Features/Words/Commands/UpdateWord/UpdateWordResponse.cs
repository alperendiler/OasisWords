namespace OasisWords.Application.Features.Words.Commands.UpdateWord;

public class UpdateWordResponse
{
    public Guid   Id   { get; set; }
    public string Text { get; set; } = string.Empty;
}
