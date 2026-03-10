using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Domain.Entities;

public class DailyTargetSession : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public DateTime Date { get; set; }
    public int TargetWordCount { get; set; }
    public int CompletedWordCount { get; set; }
    public bool IsCompleted { get; set; }

    public virtual Student Student { get; set; } = null!;
}
