namespace OasisWords.Core.Persistence.Repositories;

/// <summary>
/// Base entity with a generic primary key, created/updated audit fields.
/// </summary>
public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Convenience alias using Guid as primary key.
/// </summary>
public abstract class Entity : Entity<Guid> { }
