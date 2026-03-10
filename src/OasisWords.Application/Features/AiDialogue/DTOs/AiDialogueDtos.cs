using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.AiDialogue.DTOs;

public class DialogueMessageDto
{
    public Guid Id { get; set; }
    public MessageSender Sender { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? CorrectedText { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DialogueSessionSummaryDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public int MessageCount { get; set; }
    public int TargetWordCount { get; set; }
    public int UsedTargetWordCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DialogueSessionDetailDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Score { get; set; }
    public IList<DialogueMessageDto> Messages { get; set; } = new List<DialogueMessageDto>();
    public IList<TargetWordStatusDto> TargetWords { get; set; } = new List<TargetWordStatusDto>();
}

public class TargetWordStatusDto
{
    public string WordText { get; set; } = string.Empty;
    public string TranslationText { get; set; } = string.Empty;
    public bool IsUsedByStudent { get; set; }
}
