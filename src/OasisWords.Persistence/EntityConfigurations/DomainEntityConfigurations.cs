using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OasisWords.Domain.Entities;

namespace OasisWords.Persistence.EntityConfigurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Code).IsRequired().HasMaxLength(10);
        builder.HasIndex(l => l.Code).IsUnique();
        builder.Property(l => l.FlagImageUrl).HasMaxLength(512);
        builder.HasMany(l => l.Words)
               .WithOne(w => w.Language)
               .HasForeignKey(w => w.LanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Text).IsRequired().HasMaxLength(200);
        builder.Property(w => w.PhoneticSpelling).HasMaxLength(200);
        builder.Property(w => w.PronunciationAudioUrl).HasMaxLength(512);
        builder.HasIndex(w => new { w.LanguageId, w.Text }).IsUnique();
        builder.HasMany(w => w.Meanings)
               .WithOne(m => m.Word)
               .HasForeignKey(m => m.WordId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WordMeaningConfiguration : IEntityTypeConfiguration<WordMeaning>
{
    public void Configure(EntityTypeBuilder<WordMeaning> builder)
    {
        builder.ToTable("WordMeanings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.CefrLevel).IsRequired();
        builder.Property(m => m.TranslationText).IsRequired().HasMaxLength(500);
        builder.Property(m => m.ExampleSentence).HasMaxLength(1000);
        builder.Property(m => m.ExampleTranslation).HasMaxLength(1000);
        builder.HasIndex(m => new { m.TranslationLanguageId, m.CefrLevel });
        builder.HasOne(m => m.TranslationLanguage)
               .WithMany()
               .HasForeignKey(m => m.TranslationLanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasMany(s => s.LanguageProfiles)
               .WithOne(p => p.Student)
               .HasForeignKey(p => p.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Streak)
               .WithOne(st => st.Student)
               .HasForeignKey<StudentStreak>(st => st.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentLanguageProfileConfiguration : IEntityTypeConfiguration<StudentLanguageProfile>
{
    public void Configure(EntityTypeBuilder<StudentLanguageProfile> builder)
    {
        builder.ToTable("StudentLanguageProfiles");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TargetCefrLevel).IsRequired();
        builder.HasIndex(p => new { p.StudentId, p.NativeLanguageId, p.TargetLanguageId }).IsUnique();
        builder.HasOne(p => p.NativeLanguage)
               .WithMany()
               .HasForeignKey(p => p.NativeLanguageId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.TargetLanguage)
               .WithMany()
               .HasForeignKey(p => p.TargetLanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StudentWordProgressConfiguration : IEntityTypeConfiguration<StudentWordProgress>
{
    public void Configure(EntityTypeBuilder<StudentWordProgress> builder)
    {
        builder.ToTable("StudentWordProgresses");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.StudentId, p.WordMeaningId }).IsUnique();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.NextReviewDate).IsRequired();
        builder.Property(p => p.ConsecutiveCorrectAnswers).HasDefaultValue(0);
        builder.Property(p => p.TotalIncorrectAnswers).HasDefaultValue(0);
        builder.HasOne(p => p.Student)
               .WithMany()
               .HasForeignKey(p => p.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.WordMeaning)
               .WithMany()
               .HasForeignKey(p => p.WordMeaningId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentStreakConfiguration : IEntityTypeConfiguration<StudentStreak>
{
    public void Configure(EntityTypeBuilder<StudentStreak> builder)
    {
        builder.ToTable("StudentStreaks");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CurrentStreak).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.LongestStreak).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.LastActivityDate).IsRequired();
    }
}

public class DailyTargetSessionConfiguration : IEntityTypeConfiguration<DailyTargetSession>
{
    public void Configure(EntityTypeBuilder<DailyTargetSession> builder)
    {
        builder.ToTable("DailyTargetSessions");
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.StudentId, d.Date }).IsUnique();
        builder.HasOne(d => d.Student)
               .WithMany()
               .HasForeignKey(d => d.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiDialogueSessionConfiguration : IEntityTypeConfiguration<AiDialogueSession>
{
    public void Configure(EntityTypeBuilder<AiDialogueSession> builder)
    {
        builder.ToTable("AiDialogueSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Topic).IsRequired().HasMaxLength(300);
        builder.Property(s => s.SystemPromptContext).IsRequired();
        builder.HasOne(s => s.Student)
               .WithMany()
               .HasForeignKey(s => s.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Messages)
               .WithOne(m => m.AiDialogueSession)
               .HasForeignKey(m => m.AiDialogueSessionId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.TargetWords)
               .WithOne(t => t.AiDialogueSession)
               .HasForeignKey(t => t.AiDialogueSessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiDialogueMessageConfiguration : IEntityTypeConfiguration<AiDialogueMessage>
{
    public void Configure(EntityTypeBuilder<AiDialogueMessage> builder)
    {
        builder.ToTable("AiDialogueMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MessageText).IsRequired();
        builder.Property(m => m.Sender).IsRequired();
    }
}

public class AiDialogueTargetWordConfiguration : IEntityTypeConfiguration<AiDialogueTargetWord>
{
    public void Configure(EntityTypeBuilder<AiDialogueTargetWord> builder)
    {
        builder.ToTable("AiDialogueTargetWords");
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.AiDialogueSessionId, t.WordMeaningId }).IsUnique();
        builder.HasOne(t => t.WordMeaning)
               .WithMany()
               .HasForeignKey(t => t.WordMeaningId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
