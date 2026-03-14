namespace OasisWords.Application.Features.Words.Commands.UpdateWordMeaning;

public class UpdateWordMeaningCommand : MediatR.IRequest<UpdateWordMeaningResponse>
{
    public Guid    Id                 { get; set; }
    public string  TranslationText   { get; set; } = string.Empty;
    public string? ExampleSentence   { get; set; }
    public string? ExampleTranslation { get; set; }
}
