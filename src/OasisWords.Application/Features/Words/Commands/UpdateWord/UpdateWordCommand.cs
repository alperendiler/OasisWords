namespace OasisWords.Application.Features.Words.Commands.UpdateWord;

public class UpdateWordCommand : MediatR.IRequest<UpdateWordResponse>
{
    public Guid    Id                   { get; set; }
    public string? PhoneticSpelling     { get; set; }
    public string? PronunciationAudioUrl { get; set; }
}
