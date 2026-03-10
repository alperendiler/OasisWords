using OasisWords.Core.Persistence.Repositories;

namespace OasisWords.Domain.Entities;

public class Language : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();
}
