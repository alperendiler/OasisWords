using FluentAssertions;
using Moq;
using OasisWords.Application.Features.StudentProgress.Commands.UpdateWordProgress;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Tests.Common;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;
using System.Linq.Expressions;
using Xunit;

namespace OasisWords.Application.Tests.SRS;

public class UpdateWordProgressCommandHandlerTests
{
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid WordMeaningId = Guid.NewGuid();

    // ── Helper ────────────────────────────────────────────────────────────────

    private static UpdateWordProgressCommandHandler CreateHandler(
        StudentWordProgress? existingProgress = null)
    {
        Mock<IStudentWordProgressRepository> repo =
            MockRepositoryFactory.CreateProgressRepo(existingProgress);
        return new UpdateWordProgressCommandHandler(repo.Object);
    }

    private static UpdateWordProgressCommand CorrectCommand(int? quality = null) => new()
    {
        StudentId = StudentId,
        WordMeaningId = WordMeaningId,
        IsCorrect = true,
        Quality = quality
    };

    private static UpdateWordProgressCommand IncorrectCommand() => new()
    {
        StudentId = StudentId,
        WordMeaningId = WordMeaningId,
        IsCorrect = false
    };

    // ── First-time view tests ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FirstSeen_Correct_SetsNextReviewDateTomorrow()
    {
        var handler = CreateHandler(existingProgress: null);
        var command = CorrectCommand();

        UpdateWordProgressResponse result = await handler.Handle(command, CancellationToken.None);

        result.NextReviewDate.Date.Should().Be(DateTime.UtcNow.Date.AddDays(1),
            "first correct answer should schedule review in 1 day (SM-2 n=1)");
    }

    [Fact]
    public async Task Handle_FirstSeen_Correct_StatusIsLearning()
    {
        var handler = CreateHandler();
        var result = await handler.Handle(CorrectCommand(), CancellationToken.None);

        result.NewStatus.Should().Be(WordLearningStatus.Learning,
            "one correct answer is not enough to move beyond Learning");
    }

    [Fact]
    public async Task Handle_FirstSeen_Incorrect_SetsNextReviewDateTomorrow()
    {
        var handler = CreateHandler();
        var result = await handler.Handle(IncorrectCommand(), CancellationToken.None);

        result.NextReviewDate.Date.Should().Be(DateTime.UtcNow.Date.AddDays(1));
        result.NewStatus.Should().Be(WordLearningStatus.Learning);
    }

    // ── SM-2 interval ladder tests ────────────────────────────────────────────

    [Theory]
    [InlineData(1, 1)]   // n=1 → 1 day
    [InlineData(2, 6)]   // n=2 → 6 days
    public async Task Handle_NthCorrectAnswer_ProducesExpectedInterval(
        int repetitions, int expectedDays)
    {
        // Simulate an existing progress record that has already been reviewed (repetitions-1) times
        var existing = BuildExistingProgress(
            consecutive: repetitions - 1,
            lastReviewedAt: DateTime.UtcNow.AddDays(-(repetitions - 1)),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(CorrectCommand(), CancellationToken.None);

        result.IntervalDays.Should().Be(expectedDays,
            $"SM-2 n={repetitions} should give {expectedDays} day interval");
        result.NextReviewDate.Date.Should()
            .Be(DateTime.UtcNow.Date.AddDays(expectedDays));
    }

    [Fact]
    public async Task Handle_ThirdCorrectAnswer_IntervalIsAtLeastSixDays()
    {
        // n=3 → round(6 × EF) where EF starts at ~2.5 → ≥ 15 days
        var existing = BuildExistingProgress(
            consecutive: 2,
            lastReviewedAt: DateTime.UtcNow.AddDays(-6),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(CorrectCommand(), CancellationToken.None);

        result.IntervalDays.Should().BeGreaterThanOrEqualTo(6,
            "SM-2 n=3 interval should grow exponentially from 6 days");
    }

    // ── Incorrect answer tests ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_IncorrectAfterStreak_ResetsConsecutiveCounter()
    {
        var existing = BuildExistingProgress(consecutive: 4,
            lastReviewedAt: DateTime.UtcNow.AddDays(-14),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(IncorrectCommand(), CancellationToken.None);

        result.ConsecutiveCorrectAnswers.Should().Be(0,
            "incorrect answer must reset the streak counter");
        result.NewStatus.Should().Be(WordLearningStatus.Learning);
        result.IntervalDays.Should().Be(1);
    }

    // ── Status promotion tests ────────────────────────────────────────────────

    [Theory]
    [InlineData(1, WordLearningStatus.Learning)]
    [InlineData(2, WordLearningStatus.Reviewing)]
    [InlineData(5, WordLearningStatus.Mastered)]
    public async Task Handle_CorrectAnswers_PromotesStatusCorrectly(
        int consecutiveAfter, WordLearningStatus expectedStatus)
    {
        var existing = BuildExistingProgress(
            consecutive: consecutiveAfter - 1,
            lastReviewedAt: DateTime.UtcNow.AddDays(-1),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(CorrectCommand(), CancellationToken.None);

        result.NewStatus.Should().Be(expectedStatus,
            $"after {consecutiveAfter} correct answers status should be {expectedStatus}");
    }

    // ── Quality 0–5 parameter tests ───────────────────────────────────────────

    [Fact]
    public async Task Handle_Quality5_EasinessFactorIncreases()
    {
        var existing = BuildExistingProgress(consecutive: 1,
            lastReviewedAt: DateTime.UtcNow.AddDays(-1),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(CorrectCommand(quality: 5), CancellationToken.None);

        result.EasinessFactor.Should().BeGreaterThan(2.0,
            "perfect score should result in an above-average easiness factor");
    }

    [Fact]
    public async Task Handle_Quality2_TreatedAsIncorrect_ResetsStreak()
    {
        // Quality < 3 is a failing grade in SM-2
        var existing = BuildExistingProgress(consecutive: 3,
            lastReviewedAt: DateTime.UtcNow.AddDays(-7),
            nextReviewDate: DateTime.UtcNow.Date);

        var handler = CreateHandler(existing);
        var result = await handler.Handle(new UpdateWordProgressCommand
        {
            StudentId = StudentId,
            WordMeaningId = WordMeaningId,
            IsCorrect = false,
            Quality = 2
        }, CancellationToken.None);

        result.ConsecutiveCorrectAnswers.Should().Be(0);
        result.NewStatus.Should().Be(WordLearningStatus.Learning);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static StudentWordProgress BuildExistingProgress(
        int consecutive,
        DateTime lastReviewedAt,
        DateTime nextReviewDate)
    {
        return new StudentWordProgress
        {
            Id = Guid.NewGuid(),
            StudentId = StudentId,
            WordMeaningId = WordMeaningId,
            ConsecutiveCorrectAnswers = consecutive,
            TotalIncorrectAnswers = 0,
            LastReviewedAt = lastReviewedAt,
            NextReviewDate = nextReviewDate,
            Status = consecutive >= 5 ? WordLearningStatus.Mastered
                   : consecutive >= 2 ? WordLearningStatus.Reviewing
                   : WordLearningStatus.Learning
        };
    }
}
