using MediatR;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.StudentProgress.Queries.GetStudentDashboardStats;

public class GetStudentDashboardStatsQuery : IRequest<GetStudentDashboardStatsResponse>
{
    public Guid StudentId { get; set; }
}

public class GetStudentDashboardStatsResponse
{
    // Streak
    public int  CurrentStreak   { get; set; }
    public int  LongestStreak   { get; set; }
    public DateTime LastActivity { get; set; }

    // Günlük hedef
    public int  DailyWordGoal        { get; set; }
    public int  CompletedTodayCount  { get; set; }
    public bool DailyGoalReached     { get; set; }

    // Genel ilerleme
    public int TotalWordsLearned  { get; set; }   // Mastered
    public int TotalWordsReviewing { get; set; }  // Reviewing
    public int TotalWordsLearning  { get; set; }  // Learning
    public int DueForReviewToday  { get; set; }

    // CEFR dağılımı
    public IList<CefrProgressDto> CefrBreakdown { get; set; } = new List<CefrProgressDto>();
}

public class CefrProgressDto
{
    public string CefrLabel      { get; set; } = string.Empty;
    public int    MasteredCount  { get; set; }
    public int    LearningCount  { get; set; }
}

public class GetStudentDashboardStatsQueryHandler
    : IRequestHandler<GetStudentDashboardStatsQuery, GetStudentDashboardStatsResponse>
{
    private readonly IStudentRepository          _studentRepo;
    private readonly IStudentStreakRepository    _streakRepo;
    private readonly IStudentWordProgressRepository _progressRepo;
    private readonly IDailyTargetSessionRepository  _dailyRepo;

    public GetStudentDashboardStatsQueryHandler(
        IStudentRepository              studentRepo,
        IStudentStreakRepository        streakRepo,
        IStudentWordProgressRepository  progressRepo,
        IDailyTargetSessionRepository   dailyRepo)
    {
        _studentRepo  = studentRepo;
        _streakRepo   = streakRepo;
        _progressRepo = progressRepo;
        _dailyRepo    = dailyRepo;
    }

    public async Task<GetStudentDashboardStatsResponse> Handle(
        GetStudentDashboardStatsQuery request, CancellationToken ct)
    {
        Student student = await _studentRepo.GetAsync(
            s => s.Id == request.StudentId,
            enableTracking: false,
            cancellationToken: ct)
            ?? throw new NotFoundException("Student not found.");

        DateTime today = DateTime.UtcNow.Date;

        // Streak bilgisi
        StudentStreak? streak = await _streakRepo.GetAsync(
            s => s.StudentId == request.StudentId,
            enableTracking: false,
            cancellationToken: ct);

        // Günlük tamamlanan
        DailyTargetSession? dailySession = await _dailyRepo.GetAsync(
            d => d.StudentId == request.StudentId && d.Date.Date == today,
            enableTracking: false,
            cancellationToken: ct);

        int completedToday = dailySession?.CompletedWordCount ?? 0;

        // İlerleme istatistikleri
        var allProgress = await _progressRepo.GetListAsync(
            predicate: p => p.StudentId == request.StudentId,
            include: q => q.Include(p => p.WordMeaning),
            enableTracking: false,
            size: int.MaxValue,
            cancellationToken: ct);

        int mastered   = allProgress.Items.Count(p => p.Status == WordLearningStatus.Mastered);
        int reviewing  = allProgress.Items.Count(p => p.Status == WordLearningStatus.Reviewing);
        int learning   = allProgress.Items.Count(p => p.Status == WordLearningStatus.Learning);
        int dueToday   = allProgress.Items.Count(
            p => p.Status != WordLearningStatus.Mastered && p.NextReviewDate.Date <= today);

        // CEFR dağılımı
        var cefrBreakdown = allProgress.Items
            .GroupBy(p => p.WordMeaning.CefrLevel)
            .OrderBy(g => g.Key)
            .Select(g => new CefrProgressDto
            {
                CefrLabel     = g.Key.ToString(),
                MasteredCount = g.Count(p => p.Status == WordLearningStatus.Mastered),
                LearningCount = g.Count(p => p.Status != WordLearningStatus.Mastered)
            }).ToList();

        return new GetStudentDashboardStatsResponse
        {
            CurrentStreak         = streak?.CurrentStreak   ?? 0,
            LongestStreak         = streak?.LongestStreak   ?? 0,
            LastActivity          = streak?.LastActivityDate ?? DateTime.MinValue,
            DailyWordGoal         = student.DailyWordGoal,
            CompletedTodayCount   = completedToday,
            DailyGoalReached      = completedToday >= student.DailyWordGoal,
            TotalWordsLearned     = mastered,
            TotalWordsReviewing   = reviewing,
            TotalWordsLearning    = learning,
            DueForReviewToday     = dueToday,
            CefrBreakdown         = cefrBreakdown
        };
    }
}
