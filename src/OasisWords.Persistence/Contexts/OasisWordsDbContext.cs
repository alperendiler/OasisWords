using Microsoft.EntityFrameworkCore;
using OasisWords.Core.Security.Entities;
using OasisWords.Domain.Entities;
using System.Reflection;

namespace OasisWords.Persistence.Contexts;

public class OasisWordsDbContext : DbContext
{
    public OasisWordsDbContext(DbContextOptions<OasisWordsDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<OperationClaim> OperationClaims => Set<OperationClaim>();
    public DbSet<UserOperationClaim> UserOperationClaims => Set<UserOperationClaim>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpAuthenticator> OtpAuthenticators => Set<OtpAuthenticator>();
    public DbSet<EmailAuthenticator> EmailAuthenticators => Set<EmailAuthenticator>();

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<WordMeaning> WordMeanings => Set<WordMeaning>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentLanguageProfile> StudentLanguageProfiles => Set<StudentLanguageProfile>();
    public DbSet<StudentWordProgress> StudentWordProgresses => Set<StudentWordProgress>();
    public DbSet<StudentStreak> StudentStreaks => Set<StudentStreak>();
    public DbSet<DailyTargetSession> DailyTargetSessions => Set<DailyTargetSession>();
    public DbSet<AiDialogueSession> AiDialogueSessions => Set<AiDialogueSession>();
    public DbSet<AiDialogueMessage> AiDialogueMessages => Set<AiDialogueMessage>();
    public DbSet<AiDialogueTargetWord> AiDialogueTargetWords => Set<AiDialogueTargetWord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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
