using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Domain.Entities;

public class StudentStreak : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime LastActivityDate { get; set; }

    public virtual Student Student { get; set; } = null!;
}
