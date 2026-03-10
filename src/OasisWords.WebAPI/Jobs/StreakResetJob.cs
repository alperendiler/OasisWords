using Hangfire;
using Microsoft.EntityFrameworkCore;
using OasisWords.Domain.Entities;
using OasisWords.Persistence.Contexts;

namespace OasisWords.WebAPI.Jobs;

/// <summary>
/// Scheduled Hangfire job — runs every night at 00:05 UTC.
/// Resets CurrentStreak to 0 for any student whose LastActivityDate
/// is more than 24 hours in the past (i.e. missed yesterday entirely).
/// </summary>
public class StreakResetJob
{
    private readonly OasisWordsDbContext _db;
    private readonly ILogger<StreakResetJob> _logger;

    public StreakResetJob(OasisWordsDbContext db, ILogger<StreakResetJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Called by Hangfire on its cron schedule.
    /// We use <see cref="DisableConcurrentExecution"/> so that a slow run
    /// cannot overlap with the next scheduled invocation.
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        DateTime cutoff = DateTime.UtcNow.Date; // midnight today

        // Fetch all streaks whose last activity was BEFORE today (gap >= 1 day)
        List<StudentStreak> expiredStreaks = await _db.Set<StudentStreak>()
            .Where(s => s.CurrentStreak > 0 && s.LastActivityDate.Date < cutoff)
            .ToListAsync(cancellationToken);

        if (expiredStreaks.Count == 0)
        {
            _logger.LogInformation("StreakResetJob: no expired streaks found.");
            return;
        }

        int resetCount = 0;
        foreach (StudentStreak streak in expiredStreaks)
        {
            int daysMissed = (cutoff - streak.LastActivityDate.Date).Days;
            if (daysMissed >= 1)
            {
                _logger.LogDebug(
                    "Resetting streak for StudentId={StudentId} (last active {Days} day(s) ago).",
                    streak.StudentId, daysMissed);

                streak.CurrentStreak = 0;
                resetCount++;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StreakResetJob: reset {Count} streak(s).", resetCount);
    }
}
