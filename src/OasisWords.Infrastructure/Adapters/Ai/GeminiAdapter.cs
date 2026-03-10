using OasisWords.Application.Services.AiDialogueService;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OasisWords.Infrastructure.Adapters.Ai;

public class GeminiAdapter : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiAdapter(HttpClient httpClient, GeminiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<AiChatResponse> SendMessageAsync(
        IEnumerable<AiChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var messages = conversationHistory.ToList();

        // Gemini uses "contents" with "parts"
        var contents = messages
            .Where(m => m.Role != "system")
            .Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            })
            .ToList();

        // Prepend system instruction as first user message if present
        string? systemInstruction = messages.FirstOrDefault(m => m.Role == "system")?.Content;

        var requestBody = new
        {
            system_instruction = systemInstruction is not null
                ? new { parts = new[] { new { text = systemInstruction } } }
                : null,
            contents,
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 1024
            }
        };

        string url = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
        string json = JsonSerializer.Serialize(requestBody, JsonOptions);

        HttpResponseMessage httpResponse = await _httpClient.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        using JsonDocument doc = JsonDocument.Parse(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
        string text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        return ParseAiResponse(text);
    }

    public async Task<string> GenerateSystemContextAsync(
        string topic,
        IEnumerable<string> targetWords,
        CancellationToken cancellationToken = default)
    {
        string wordList = string.Join(", ", targetWords.Select(w => $"'{w}'"));

        string prompt =
            $"You are a conversational English teacher. Your role is to have a natural conversation " +
            $"about the topic: '{topic}'. During the conversation, naturally use and encourage the student " +
            $"to use these target vocabulary words: {wordList}. " +
            $"When the student makes grammar or vocabulary errors, gently correct them. " +
            $"After 6-8 exchanges, evaluate the student's performance and provide a score from 0-100 " +
            $"based on grammar accuracy and vocabulary usage. " +
            $"When providing a final score, include it in the format: [SCORE:XX] at the end of your message.";

        AiChatResponse response = await SendMessageAsync(
            new[] { new AiChatMessage { Role = "user", Content = $"Generate system context: {prompt}" } },
            cancellationToken);

        return response.Content.Length > 0 ? response.Content : prompt;
    }

    private static AiChatResponse ParseAiResponse(string rawText)
    {
        string? correctedText = null;
        int? score = null;

        // Extract correction if present: [CORRECTION: ...]
        int corrStart = rawText.IndexOf("[CORRECTION:", StringComparison.OrdinalIgnoreCase);
        if (corrStart >= 0)
        {
            int corrEnd = rawText.IndexOf(']', corrStart);
            if (corrEnd > corrStart)
            {
                correctedText = rawText[(corrStart + 12)..corrEnd].Trim();
                rawText = rawText.Remove(corrStart, corrEnd - corrStart + 1).Trim();
            }
        }

        // Extract score if present: [SCORE:XX]
        int scoreStart = rawText.IndexOf("[SCORE:", StringComparison.OrdinalIgnoreCase);
        if (scoreStart >= 0)
        {
            int scoreEnd = rawText.IndexOf(']', scoreStart);
            if (scoreEnd > scoreStart)
            {
                string scoreStr = rawText[(scoreStart + 7)..scoreEnd].Trim();
                if (int.TryParse(scoreStr, out int parsedScore))
                    score = Math.Clamp(parsedScore, 0, 100);

                rawText = rawText.Remove(scoreStart, scoreEnd - scoreStart + 1).Trim();
            }
        }

        return new AiChatResponse
        {
            Content = rawText,
            CorrectedStudentText = correctedText,
            Score = score
        };
    }
}
