using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Commands.UpdateWordMeaning;

public class UpdateWordMeaningResponse
{
    public Guid      Id              { get; set; }
    public string    TranslationText { get; set; } = string.Empty;
    public CefrLevel CefrLevel       { get; set; }
}
