using Microsoft.EntityFrameworkCore;
using OasisWords.Core.Security.Entities;
using System.Reflection;

namespace OasisWords.Persistence.Contexts;

public class OasisWordsDbContext : DbContext
{
    public OasisWordsDbContext(DbContextOptions<OasisWordsDbContext> options)
        : base(options) { }

    // ── Security DbSets ───────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<OperationClaim> OperationClaims => Set<OperationClaim>();
    public DbSet<UserOperationClaim> UserOperationClaims => Set<UserOperationClaim>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpAuthenticator> OtpAuthenticators => Set<OtpAuthenticator>();
    public DbSet<EmailAuthenticator> EmailAuthenticators => Set<EmailAuthenticator>();

    // ── Domain DbSets – add here as you create domain entities ───────────
    // public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply soft-delete filter across all entities that expose DeletedAt
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("DeletedAt") is not null)
            {
                var method = typeof(OasisWordsDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set CreatedAt / UpdatedAt on save
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") is not null)
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;

            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") is not null)
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
