using CsvHelper.Configuration.Attributes;

namespace OasisWords.DataSeeder.Models;

/// <summary>
/// Maps a row in the Oxford 3000/5000 CSV file.
/// Expected columns: Word, PartOfSpeech, CefrLevel
/// Example CSV header: word,pos,cefr
/// </summary>
public class EnglishCefrWord 
{
    [Name("word")]
    public string Word { get; set; } = string.Empty;

    // Eðer CSV'nde bu kolon yoksa [Optional] eklemelisin ki program çökmesin
    [Name("pos")]
    [Optional]
    public string? PartOfSpeech { get; set; }

    [Name("CEFR")] // CSV'deki büyük harfli yazýma tam uyum
    public string Cefr { get; set; } = string.Empty;
}
public class SeederSettings
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-1.5-flash";
    public string GeminiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public string CsvFilePath { get; set; } = "Data/ENGLISH_CERF_WORDS.csv";

    /// <summary>Guid of the English language row (must match seed data)</summary>
    public Guid EnglishLanguageId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Guid of the Turkish language row (must match seed data)</summary>
    public Guid TurkishLanguageId { get; set; } = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>Words processed per Gemini batch request (keep low to stay under token limits)</summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>Milliseconds to wait between Gemini batches (rate-limit safety)</summary>
    public int DelayBetweenBatchesMs { get; set; } = 2000;
}
