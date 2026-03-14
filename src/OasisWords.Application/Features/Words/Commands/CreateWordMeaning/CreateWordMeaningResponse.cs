using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Commands.CreateWordMeaning;

public class CreateWordMeaningResponse
{
    public Guid Id { get; set; }
    public Guid WordId { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
}
