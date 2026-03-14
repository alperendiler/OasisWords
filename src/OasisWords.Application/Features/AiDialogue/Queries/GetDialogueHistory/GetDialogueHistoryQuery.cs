using MediatR;
using OasisWords.Application.Features.AiDialogue.DTOs;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Core.Application.Requests;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.AiDialogue.Queries.GetDialogueHistory;

public class GetDialogueHistoryQuery : IRequest<GetDialogueHistoryResponse>
{
    public Guid StudentId { get; set; }
    public PageRequest PageRequest { get; set; } = new();
}

public class GetDialogueSessionDetailQuery : IRequest<DialogueSessionDetailDto>
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
}

public class GetDialogueHistoryResponse
{
    public IList<DialogueSessionSummaryDto> Items { get; set; } = new List<DialogueSessionSummaryDto>();
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
}

public class GetDialogueHistoryQueryHandler : IRequestHandler<GetDialogueHistoryQuery, GetDialogueHistoryResponse>
{
    private readonly IAiDialogueSessionRepository _sessionRepository;

    public GetDialogueHistoryQueryHandler(IAiDialogueSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<GetDialogueHistoryResponse> Handle(
        GetDialogueHistoryQuery request,
        CancellationToken cancellationToken)
    {
        IPaginate<AiDialogueSession> sessions = await _sessionRepository.GetListAsync(
            predicate: s => s.StudentId == request.StudentId,
            include: q => q
                .Include(s => s.Messages)
                .Include(s => s.TargetWords),
            orderBy: q => q.OrderByDescending(s => s.CreatedAt),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        IList<DialogueSessionSummaryDto> items = sessions.Items.Select(s => new DialogueSessionSummaryDto
        {
            Id = s.Id,
            Topic = s.Topic,
            IsCompleted = s.IsCompleted,
            Score = s.Score,
            MessageCount = s.Messages.Count,
            TargetWordCount = s.TargetWords.Count,
            UsedTargetWordCount = s.TargetWords.Count(t => t.IsUsedByStudent),
            CreatedAt = s.CreatedAt
        }).ToList();

        return new GetDialogueHistoryResponse
        {
            Items = items,
            Index = sessions.Index,
            Size = sessions.Size,
            Count = sessions.Count,
            Pages = sessions.Pages
        };
    }
}

public class GetDialogueSessionDetailQueryHandler : IRequestHandler<GetDialogueSessionDetailQuery, DialogueSessionDetailDto>
{
    private readonly IAiDialogueSessionRepository _sessionRepository;

    public GetDialogueSessionDetailQueryHandler(IAiDialogueSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<DialogueSessionDetailDto> Handle(
        GetDialogueSessionDetailQuery request,
        CancellationToken cancellationToken)
    {
        AiDialogueSession? session = await _sessionRepository.GetAsync(
            s => s.Id == request.SessionId && s.StudentId == request.StudentId,
            include: q => q
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .Include(s => s.TargetWords)
                    .ThenInclude(t => t.WordMeaning)
                        .ThenInclude(m => m.Word),
            enableTracking: false,
            cancellationToken: cancellationToken)
            ?? throw new OasisWords.Core.CrossCuttingConcerns.Exceptions.NotFoundException("Session not found.");

        return new DialogueSessionDetailDto
        {
            Id = session.Id,
            Topic = session.Topic,
            IsCompleted = session.IsCompleted,
            Score = session.Score,
            Messages = session.Messages.Select(m => new DialogueMessageDto
            {
                Id = m.Id,
                Sender = m.Sender,
                MessageText = m.MessageText,
                CorrectedText = m.CorrectedText,
                CreatedAt = m.CreatedAt
            }).ToList(),
            TargetWords = session.TargetWords.Select(t => new TargetWordStatusDto
            {
                WordText = t.WordMeaning.Word.Text,
                TranslationText = t.WordMeaning.TranslationText,
                IsUsedByStudent = t.IsUsedByStudent
            }).ToList()
        };
    }
}
