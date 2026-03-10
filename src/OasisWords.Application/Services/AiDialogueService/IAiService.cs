namespace OasisWords.Application.Services.AiDialogueService;

public class AiChatMessage
{
    public string Role { get; set; } = string.Empty; // "system" | "user" | "assistant"
    public string Content { get; set; } = string.Empty;
}

public class AiChatResponse
{
    public string Content { get; set; } = string.Empty;
    public string? CorrectedStudentText { get; set; }
    public int? Score { get; set; }
}

public interface IAiService
{
    Task<AiChatResponse> SendMessageAsync(
        IEnumerable<AiChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);

    Task<string> GenerateSystemContextAsync(
        string topic,
        IEnumerable<string> targetWords,
        CancellationToken cancellationToken = default);
}
