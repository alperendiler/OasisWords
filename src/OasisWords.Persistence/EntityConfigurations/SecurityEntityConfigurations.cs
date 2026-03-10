using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OasisWords.Core.Security.Entities;

namespace OasisWords.Persistence.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.PasswordSalt).IsRequired();
        builder.HasMany(u => u.UserOperationClaims)
               .WithOne(uoc => uoc.User)
               .HasForeignKey(uoc => uoc.UserId);
        builder.HasMany(u => u.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId);
        builder.HasOne(u => u.OtpAuthenticator)
               .WithOne(o => o.User)
               .HasForeignKey<OtpAuthenticator>(o => o.UserId);
        builder.HasOne(u => u.EmailAuthenticator)
               .WithOne(e => e.User)
               .HasForeignKey<EmailAuthenticator>(e => e.UserId);
    }
}

public class OperationClaimConfiguration : IEntityTypeConfiguration<OperationClaim>
{
    // Stable Guids for seed roles
    public static readonly Guid AdminRoleId  = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    public static readonly Guid StudentRoleId = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");

    public void Configure(EntityTypeBuilder<OperationClaim> builder)
    {
        builder.ToTable("OperationClaims");
        builder.HasKey(oc => oc.Id);
        builder.Property(oc => oc.Name).IsRequired().HasMaxLength(256);
        builder.HasIndex(oc => oc.Name).IsUnique();

        // ── Seed Data ──────────────────────────────────────────────────────────
        builder.HasData(
            new OperationClaim
            {
                Id = AdminRoleId,
                Name = "Admin",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new OperationClaim
            {
                Id = StudentRoleId,
                Name = "Student",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}

public class UserOperationClaimConfiguration : IEntityTypeConfiguration<UserOperationClaim>
{
    public void Configure(EntityTypeBuilder<UserOperationClaim> builder)
    {
        builder.ToTable("UserOperationClaims");
        builder.HasKey(uoc => uoc.Id);
        builder.HasIndex(uoc => new { uoc.UserId, uoc.OperationClaimId }).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
    }
}
