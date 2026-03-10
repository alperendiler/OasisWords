using CsvHelper.Configuration.Attributes;

namespace OasisWords.DataSeeder.Models;

/// <summary>
/// Maps a row in the Oxford 3000/5000 CSV file.
/// Expected columns: Word, PartOfSpeech, CefrLevel
/// Example CSV header: word,pos,cefr
/// </summary>
public class OxfordWordRecord
{
    [Name("word")]
    public string Word { get; set; } = string.Empty;

    [Name("pos")]
    public string PartOfSpeech { get; set; } = string.Empty;

    /// <summary>Raw CEFR string from CSV: "a1", "a2", "b1", "b2", "c1", "c2"</summary>
    [Name("cefr")]
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
    public string CsvFilePath { get; set; } = "Data/oxford_words.csv";

    /// <summary>Guid of the English language row (must match seed data)</summary>
    public Guid EnglishLanguageId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Guid of the Turkish language row (must match seed data)</summary>
    public Guid TurkishLanguageId { get; set; } = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>Words processed per Gemini batch request (keep low to stay under token limits)</summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>Milliseconds to wait between Gemini batches (rate-limit safety)</summary>
    public int DelayBetweenBatchesMs { get; set; } = 2000;
}
