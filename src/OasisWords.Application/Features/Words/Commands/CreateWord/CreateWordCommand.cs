namespace OasisWords.Application.Features.Words.Commands.CreateWord;

public class CreateWordCommand : MediatR.IRequest<CreateWordResponse>
{
    public Guid LanguageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }
}
