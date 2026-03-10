using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Domain.Entities;

public class Word : Entity<Guid>
{
    public Guid LanguageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhoneticSpelling { get; set; }
    public string? PronunciationAudioUrl { get; set; }

    public virtual Language Language { get; set; } = null!;
    public virtual ICollection<WordMeaning> Meanings { get; set; } = new List<WordMeaning>();
}

public class WordMeaning : Entity<Guid>
{
    public Guid WordId { get; set; }
    public Guid TranslationLanguageId { get; set; }
    public string CefrLevel { get; set; } = string.Empty;
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }

    public virtual Word Word { get; set; } = null!;
    public virtual Language TranslationLanguage { get; set; } = null!;
}
