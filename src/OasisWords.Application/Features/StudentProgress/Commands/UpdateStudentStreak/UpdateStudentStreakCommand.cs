using MediatR;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.StudentProgress.Commands.UpdateStudentStreak;

public class UpdateStudentStreakCommand : IRequest<UpdateStudentStreakResponse>
{
    public Guid StudentId { get; set; }
}

public class UpdateStudentStreakResponse
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public bool StreakExtended { get; set; }
    public bool StreakBroken { get; set; }
}

public class UpdateStudentStreakCommandHandler : IRequestHandler<UpdateStudentStreakCommand, UpdateStudentStreakResponse>
{
    private readonly IStudentStreakRepository _streakRepository;
    private readonly IDailyTargetSessionRepository _dailyTargetSessionRepository;

    public UpdateStudentStreakCommandHandler(
        IStudentStreakRepository streakRepository,
        IDailyTargetSessionRepository dailyTargetSessionRepository)
    {
        _streakRepository = streakRepository;
        _dailyTargetSessionRepository = dailyTargetSessionRepository;
    }

    public async Task<UpdateStudentStreakResponse> Handle(
        UpdateStudentStreakCommand request,
        CancellationToken cancellationToken)
    {
        DateTime today = DateTime.UtcNow.Date;

        StudentStreak? streak = await _streakRepository.GetAsync(
            s => s.StudentId == request.StudentId,
            cancellationToken: cancellationToken);

        bool streakExtended = false;
        bool streakBroken = false;

        if (streak is null)
        {
            streak = new StudentStreak
            {
                StudentId = request.StudentId,
                CurrentStreak = 1,
                LongestStreak = 1,
                LastActivityDate = today
            };
            await _streakRepository.AddAsync(streak, cancellationToken);
            streakExtended = true;
        }
        else
        {
            DateTime lastActivity = streak.LastActivityDate.Date;
            int daysSinceLast = (today - lastActivity).Days;

            if (daysSinceLast == 0)
            {
                // Already updated today — no change
            }
            else if (daysSinceLast == 1)
            {
                // Consecutive day — extend streak
                streak.CurrentStreak++;
                streak.LastActivityDate = today;
                if (streak.CurrentStreak > streak.LongestStreak)
                    streak.LongestStreak = streak.CurrentStreak;
                streakExtended = true;
                await _streakRepository.UpdateAsync(streak, cancellationToken);
            }
            else
            {
                // Gap in activity — reset streak
                streak.CurrentStreak = 1;
                streak.LastActivityDate = today;
                streakBroken = true;
                await _streakRepository.UpdateAsync(streak, cancellationToken);
            }
        }

        // Update or create today's DailyTargetSession
        DailyTargetSession? session = await _dailyTargetSessionRepository.GetAsync(
            s => s.StudentId == request.StudentId && s.Date.Date == today,
            cancellationToken: cancellationToken);

        if (session is null)
        {
            session = new DailyTargetSession
            {
                StudentId = request.StudentId,
                Date = today,
                TargetWordCount = 0,
                CompletedWordCount = 1,
                IsCompleted = false
            };
            await _dailyTargetSessionRepository.AddAsync(session, cancellationToken);
        }
        else
        {
            session.CompletedWordCount++;
            await _dailyTargetSessionRepository.UpdateAsync(session, cancellationToken);
        }

        return new UpdateStudentStreakResponse
        {
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            StreakExtended = streakExtended,
            StreakBroken = streakBroken
        };
    }
}
