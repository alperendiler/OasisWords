using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Enums;

namespace OasisWords.Domain.Entities;

public class AiDialogueSession : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string SystemPromptContext { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Score { get; set; }

    public virtual Student Student { get; set; } = null!;
    public virtual ICollection<AiDialogueMessage> Messages { get; set; } = new List<AiDialogueMessage>();
    public virtual ICollection<AiDialogueTargetWord> TargetWords { get; set; } = new List<AiDialogueTargetWord>();
}

public class AiDialogueMessage : Entity<Guid>
{
    public Guid AiDialogueSessionId { get; set; }
    public MessageSender Sender { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? CorrectedText { get; set; }

    public virtual AiDialogueSession AiDialogueSession { get; set; } = null!;
}

public class AiDialogueTargetWord : Entity<Guid>
{
    public Guid AiDialogueSessionId { get; set; }
    public Guid WordMeaningId { get; set; }
    public bool IsUsedByStudent { get; set; }

    public virtual AiDialogueSession AiDialogueSession { get; set; } = null!;
    public virtual WordMeaning WordMeaning { get; set; } = null!;
}
