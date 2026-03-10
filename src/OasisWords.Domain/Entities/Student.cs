using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Enums;

namespace OasisWords.Domain.Entities;

public class Student : Entity<Guid>
{
    public Guid UserId { get; set; }
    public int DailyWordGoal { get; set; }

    public virtual ICollection<StudentLanguageProfile> LanguageProfiles { get; set; } = new List<StudentLanguageProfile>();
    public virtual StudentStreak? Streak { get; set; }
}

public class StudentLanguageProfile : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid NativeLanguageId { get; set; }
    public Guid TargetLanguageId { get; set; }
    public CefrLevel TargetCefrLevel { get; set; }

    public virtual Student Student { get; set; } = null!;
    public virtual Language NativeLanguage { get; set; } = null!;
    public virtual Language TargetLanguage { get; set; } = null!;
}
