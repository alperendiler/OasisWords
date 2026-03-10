using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Enums;

namespace OasisWords.Domain.Entities;

public class StudentWordProgress : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid WordMeaningId { get; set; }
    public WordLearningStatus Status { get; set; }
    public DateTime NextReviewDate { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public int ConsecutiveCorrectAnswers { get; set; }
    public int TotalIncorrectAnswers { get; set; }

    public virtual Student Student { get; set; } = null!;
    public virtual WordMeaning WordMeaning { get; set; } = null!;
}
