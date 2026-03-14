using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Commands.CreateWordMeaning;

public class CreateWordMeaningCommand : MediatR.IRequest<CreateWordMeaningResponse>
{
    public Guid WordId { get; set; }
    public Guid TranslationLanguageId { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
}
