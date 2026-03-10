using MediatR;
using OasisWords.Application.Features.StudentProgress.DTOs;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.StudentProgress.Queries.GetDailyTargetWords;

public class GetDailyTargetWordsQuery : IRequest<GetDailyTargetWordsResponse>
{
    public Guid StudentId { get; set; }
}

public class GetDailyTargetWordsResponse
{
    public IList<DailyTargetWordDto> Words { get; set; } = new List<DailyTargetWordDto>();
    public int TotalTarget { get; set; }
    public int DueForReview { get; set; }
    public int NewWords { get; set; }
}

public class GetDailyTargetWordsQueryHandler : IRequestHandler<GetDailyTargetWordsQuery, GetDailyTargetWordsResponse>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentWordProgressRepository _progressRepository;
    private readonly IWordMeaningRepository _wordMeaningRepository;

    public GetDailyTargetWordsQueryHandler(
        IStudentRepository studentRepository,
        IStudentWordProgressRepository progressRepository,
        IWordMeaningRepository wordMeaningRepository)
    {
        _studentRepository = studentRepository;
        _progressRepository = progressRepository;
        _wordMeaningRepository = wordMeaningRepository;
    }

    public async Task<GetDailyTargetWordsResponse> Handle(
        GetDailyTargetWordsQuery request,
        CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetAsync(
            s => s.Id == request.StudentId,
            include: q => q.Include(s => s.LanguageProfiles),
            enableTracking: false,
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Student not found.");

        DateTime today = DateTime.UtcNow.Date;

        // Words due for review (NextReviewDate <= today, already in progress)
        var dueProgresses = await _progressRepository.GetListAsync(
            predicate: p => p.StudentId == request.StudentId
                         && p.Status != WordLearningStatus.Mastered
                         && p.NextReviewDate.Date <= today,
            include: q => q
                .Include(p => p.WordMeaning)
                    .ThenInclude(m => m.Word)
                .Include(p => p.WordMeaning)
                    .ThenInclude(m => m.TranslationLanguage),
            enableTracking: false,
            cancellationToken: cancellationToken);

        int reviewCount = dueProgresses.Items.Count;
        int newWordsNeeded = Math.Max(0, student.DailyWordGoal - reviewCount);

        // Collect already-known WordMeaning IDs to exclude
        var knownIds = new HashSet<Guid>(
            dueProgresses.Items.Select(p => p.WordMeaningId));

        // Existing progress IDs (mastered + in progress) to avoid re-surfacing
        var allProgressIds = await _progressRepository.GetListAsync(
            predicate: p => p.StudentId == request.StudentId,
            enableTracking: false,
            size: int.MaxValue,
            cancellationToken: cancellationToken);

        var excludeIds = new HashSet<Guid>(allProgressIds.Items.Select(p => p.WordMeaningId));

        // Pull new unseen words based on student's target language profile
        Guid? targetLanguageId = student.LanguageProfiles.FirstOrDefault()?.TargetLanguageId;

        var newMeanings = await _wordMeaningRepository.GetListAsync(
            predicate: m => !excludeIds.Contains(m.Id)
                         && (targetLanguageId == null || m.Word.LanguageId == targetLanguageId),
            include: q => q
                .Include(m => m.Word)
                .Include(m => m.TranslationLanguage),
            orderBy: q => q.OrderBy(m => m.CefrLevel).ThenBy(m => Guid.NewGuid()),
            index: 0,
            size: newWordsNeeded,
            enableTracking: false,
            cancellationToken: cancellationToken);

        var result = new List<DailyTargetWordDto>();

        // Map due-for-review words
        foreach (var progress in dueProgresses.Items)
        {
            result.Add(MapToDto(progress.WordMeaning, progress.Status, isNew: false));
        }

        // Map new words
        foreach (var meaning in newMeanings.Items)
        {
            result.Add(MapToDto(meaning, WordLearningStatus.New, isNew: true));
        }

        return new GetDailyTargetWordsResponse
        {
            Words = result,
            TotalTarget = student.DailyWordGoal,
            DueForReview = reviewCount,
            NewWords = newMeanings.Items.Count
        };
    }

    private static DailyTargetWordDto MapToDto(WordMeaning meaning, WordLearningStatus status, bool isNew)
    {
        return new DailyTargetWordDto
        {
            WordMeaningId = meaning.Id,
            WordText = meaning.Word.Text,
            PhoneticSpelling = meaning.Word.PhoneticSpelling,
            PronunciationAudioUrl = meaning.Word.PronunciationAudioUrl,
            CefrLevel = meaning.CefrLevel,
            TranslationText = meaning.TranslationText,
            ExampleSentence = meaning.ExampleSentence,
            ExampleTranslation = meaning.ExampleTranslation,
            Status = status,
            IsNew = isNew
        };
    }
}
