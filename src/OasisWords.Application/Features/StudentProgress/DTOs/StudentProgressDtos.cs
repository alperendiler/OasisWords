using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.StudentProgress.DTOs;

public class DailyTargetWordDto
{
    public Guid WordMeaningId { get; set; }
    public string WordText { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
    public WordLearningStatus Status { get; set; }
    public bool IsNew { get; set; }
}
