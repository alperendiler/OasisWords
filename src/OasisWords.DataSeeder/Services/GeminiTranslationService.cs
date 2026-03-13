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
        IEnumerable<EnglishCefrWord> words,
        CancellationToken ct = default)
    {
        string wordList = string.Join("\n", words.Select(w => $"- {w.Word} ({w.Cefr})"));

        string prompt = $$"""
You are an English-Turkish dictionary AI. For each English word below, provide:
1. A contextual Turkish translation (not a raw dictionary entry — choose the most common meaning)
2. A natural English example sentence
3. The Turkish translation of that example sentence

Return ONLY a valid JSON array with no markdown fences, no extra text.
Each element must have these exact keys: word, cefr, translationTr, exampleSentence, exampleTranslation

Words to translate:
{{wordList}} 

Example output format:
[
  {
    "word": "abandon",
    "cefr": "b2",
    "translationTr": "terk etmek",
    "exampleSentence": "She had to abandon her car in the snow.",
    "exampleTranslation": "Arabayı karda bırakmak zorunda kaldı."
  }
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
        int maxRetries = 3;
        int delayMs = 10000; // İlk hata alındığında 10 saniye bekle

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            HttpResponseMessage response = await _http.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json"),
                ct);

            if (response.IsSuccessStatusCode)
            {
                // Başarılıysa json'ı parse et ve metottan çık
                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
                string rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "[]";

                rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

                try
                {
                    return JsonSerializer.Deserialize<List<GeminiTranslation>>(rawText, JsonOpts) ?? new();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Failed to parse Gemini response: {Error}", ex.Message);
                    return new();
                }
            }

            // Başarısızsa hatayı oku
            string errorContent = await response.Content.ReadAsStringAsync(ct);

            // Eğer Sunucu Yoğunsa (503) veya Çok İstek Attıysak (429)
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Gemini API Meşgul! (Deneme {Attempt}/{MaxRetries}). {Wait} saniye bekleniyor...", attempt, maxRetries, delayMs / 1000);

                if (attempt == maxRetries) break; // Son denemeydi, pes et

                await Task.Delay(delayMs, ct);
                delayMs *= 2; // Bir sonraki denemede bekleme süresini ikiye katla (10s -> 20s)
                continue; // Döngünün başına dön ve tekrar istek at
            }

            // Başka bir kalıcı hataysa (örn: API Key yanlış vs.) direkt programı uyar
            _logger.LogCritical("\n=== GEMINI API REDDETTİ ===\nHTTP: {Status}\nDetay: {Error}\n===========================\n", response.StatusCode, errorContent);
            return new();
        }

        _logger.LogError("Gemini API'ye {Max} kez bağlanılamadı, bu parti atlanıyor.", maxRetries);
        return new();
    }
}
