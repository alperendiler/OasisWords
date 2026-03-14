using MediatR;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.Application.Pipelines;
using OasisWords.Core.Application.Requests;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.StudentProgress.Queries.GetLearnedWords;

/// <summary>
/// Öğrencinin öğrendiği (Mastered) kelimeleri döner.
/// Opsiyonel CEFR filtresiyle daraltılabilir.
/// Sonuçlar cache'lenir (per-student, 15 dakika).
/// </summary>
public class GetLearnedWordsQuery : IRequest<GetLearnedWordsResponse>, ICachableRequest
{
    public Guid       StudentId  { get; set; }
    public CefrLevel? CefrFilter { get; set; }
    public PageRequest PageRequest { get; set; } = new();

    public string  CacheKey         => $"LearnedWords_{StudentId}_{CefrFilter}_{PageRequest.PageIndex}_{PageRequest.PageSize}";
    public bool    BypassCache       => false;
    public string? CacheGroupKey     => $"StudentProgress_{StudentId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
}

public class GetLearnedWordsResponse
{
    public IList<LearnedWordDto> Items { get; set; } = new List<LearnedWordDto>();
    public int Count  { get; set; }
    public int Pages  { get; set; }
    public int Index  { get; set; }
    public int Size   { get; set; }
}

public class LearnedWordDto
{
    public Guid      WordMeaningId      { get; set; }
    public string    WordText           { get; set; } = string.Empty;
    public string?   PhoneticSpelling   { get; set; }
    public CefrLevel CefrLevel          { get; set; }
    public string    CefrLabel          { get; set; } = string.Empty;
    public string    TranslationText    { get; set; } = string.Empty;
    public string?   ExampleSentence    { get; set; }
    public DateTime  MasteredAt         { get; set; }
    public int       TotalCorrect       { get; set; }
}

public class GetLearnedWordsQueryHandler : IRequestHandler<GetLearnedWordsQuery, GetLearnedWordsResponse>
{
    private readonly IStudentWordProgressRepository _repo;

    public GetLearnedWordsQueryHandler(IStudentWordProgressRepository repo) => _repo = repo;

    public async Task<GetLearnedWordsResponse> Handle(GetLearnedWordsQuery request, CancellationToken ct)
    {
        IPaginate<StudentWordProgress> page = await _repo.GetListAsync(
            predicate: p =>
                p.StudentId == request.StudentId
                && p.Status == WordLearningStatus.Mastered
                && (request.CefrFilter == null || p.WordMeaning.CefrLevel == request.CefrFilter),
            include: q => q
                .Include(p => p.WordMeaning)
                    .ThenInclude(m => m.Word),
            orderBy: q => q.OrderByDescending(p => p.LastReviewedAt),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: ct);

        return new GetLearnedWordsResponse
        {
            Items = page.Items.Select(p => new LearnedWordDto
            {
                WordMeaningId    = p.WordMeaningId,
                WordText         = p.WordMeaning.Word.Text,
                PhoneticSpelling = p.WordMeaning.Word.PhoneticSpelling,
                CefrLevel        = p.WordMeaning.CefrLevel,
                CefrLabel        = p.WordMeaning.CefrLevel.ToString(),
                TranslationText  = p.WordMeaning.TranslationText,
                ExampleSentence  = p.WordMeaning.ExampleSentence,
                MasteredAt       = p.LastReviewedAt ?? p.CreatedAt,
                TotalCorrect     = p.ConsecutiveCorrectAnswers
            }).ToList(),
            Count = page.Count,
            Pages = page.Pages,
            Index = page.Index,
            Size  = page.Size
        };
    }
}
