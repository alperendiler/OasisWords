using OasisWords.DataSeeder.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OasisWords.DataSeeder.Services;

public class GeminiTranslationService
{
    private readonly HttpClient _http;
    private readonly SeederSettings _settings;
    private readonly ILogger<GeminiTranslationService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public GeminiTranslationService(
        HttpClient http,
        SeederSettings settings,
        ILogger<GeminiTranslationService> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Sends a batch of words to Gemini and returns contextual Turkish translations.
    /// Instructs the model to return ONLY a JSON array — no markdown fences.
    /// </summary>
    public async Task<List<GeminiTranslation>> TranslateBatchAsync(
        IEnumerable<OxfordWordRecord> words,
        CancellationToken ct = default)
    {
        string wordList = string.Join("\n", words.Select(w => $"- {w.Word} ({w.Cefr})"));

        string prompt = $"""
            You are an English-Turkish dictionary AI. For each English word below, provide:
            1. A contextual Turkish translation (not a raw dictionary entry — choose the most common meaning)
            2. A natural English example sentence
            3. The Turkish translation of that example sentence

            Return ONLY a valid JSON array with no markdown fences, no extra text.
            Each element must have these exact keys: word, cefr, translationTr, exampleSentence, exampleTranslation

            Words to translate:
            {wordList}

            Example output format:
            [
              {{
                "word": "abandon",
                "cefr": "b2",
                "translationTr": "terk etmek",
                "exampleSentence": "She had to abandon her car in the snow.",
                "exampleTranslation": "Arabayı karda bırakmak zorunda kaldı."
              }}
            ]
            """;

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = "You are a professional English-Turkish translation AI. Always return valid JSON only." } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = 4096
            }
        };

        string url = $"{_settings.GeminiBaseUrl}/models/{_settings.GeminiModel}:generateContent?key={_settings.GeminiApiKey}";
        string json = JsonSerializer.Serialize(requestBody, JsonOpts);

        HttpResponseMessage response = await _http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct);

        response.EnsureSuccessStatusCode();

        using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        string rawText = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "[]";

        // Strip any accidental markdown fences
        rawText = rawText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            return JsonSerializer.Deserialize<List<GeminiTranslation>>(rawText, JsonOpts)
                   ?? new List<GeminiTranslation>();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Failed to parse Gemini response: {Error}\nRaw: {Raw}", ex.Message, rawText);
            return new List<GeminiTranslation>();
        }
    }
}
