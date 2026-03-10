using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.DTOs;

public class WordListItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public int MeaningCount { get; set; }
}

public class WordDetailDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public List<WordMeaningDto> Meanings { get; set; } = new();
}

public class WordMeaningDto
{
    public Guid Id { get; set; }
    public CefrLevel CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
    public string TranslationLanguageName { get; set; } = string.Empty;
}
