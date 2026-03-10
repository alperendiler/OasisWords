using FluentValidation;
using MediatR;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.AiDialogue.Commands.StartDialogueSession;

public class StartDialogueSessionCommand : IRequest<StartDialogueSessionResponse>
{
    public Guid StudentId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int TargetWordCount { get; set; } = 5;
}

public class StartDialogueSessionResponse
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string OpeningMessage { get; set; } = string.Empty;
    public IList<string> TargetWords { get; set; } = new List<string>();
}

public class StartDialogueSessionCommandHandler : IRequestHandler<StartDialogueSessionCommand, StartDialogueSessionResponse>
{
    private readonly IAiDialogueSessionRepository _sessionRepository;
    private readonly IAiDialogueMessageRepository _messageRepository;
    private readonly IAiDialogueTargetWordRepository _targetWordRepository;
    private readonly IStudentWordProgressRepository _progressRepository;
    private readonly IWordMeaningRepository _wordMeaningRepository;
    private readonly IAiService _aiService;

    public StartDialogueSessionCommandHandler(
        IAiDialogueSessionRepository sessionRepository,
        IAiDialogueMessageRepository messageRepository,
        IAiDialogueTargetWordRepository targetWordRepository,
        IStudentWordProgressRepository progressRepository,
        IWordMeaningRepository wordMeaningRepository,
        IAiService aiService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _targetWordRepository = targetWordRepository;
        _progressRepository = progressRepository;
        _wordMeaningRepository = wordMeaningRepository;
        _aiService = aiService;
    }

    public async Task<StartDialogueSessionResponse> Handle(
        StartDialogueSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Select recently-learned words to reinforce in dialogue
        var recentProgress = await _progressRepository.GetListAsync(
            predicate: p => p.StudentId == request.StudentId
                         && (p.Status == WordLearningStatus.Learning || p.Status == WordLearningStatus.Reviewing),
            include: q => q
                .Include(p => p.WordMeaning)
                    .ThenInclude(m => m.Word),
            orderBy: q => q.OrderByDescending(p => p.LastReviewedAt),
            index: 0,
            size: request.TargetWordCount,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (!recentProgress.Items.Any())
            throw new BusinessException("No words available for dialogue. Complete some vocabulary exercises first.");

        List<WordMeaning> targetMeanings = recentProgress.Items
            .Select(p => p.WordMeaning)
            .ToList();

        List<string> targetWordTexts = targetMeanings
            .Select(m => m.Word.Text)
            .ToList();

        // Generate system context via AI
        string systemContext = await _aiService.GenerateSystemContextAsync(
            request.Topic,
            targetWordTexts,
            cancellationToken);

        // Create session
        AiDialogueSession session = new()
        {
            StudentId = request.StudentId,
            Topic = request.Topic,
            SystemPromptContext = systemContext,
            IsCompleted = false,
            Score = 0
        };
        session = await _sessionRepository.AddAsync(session, cancellationToken);

        // Save target words
        foreach (WordMeaning meaning in targetMeanings)
        {
            await _targetWordRepository.AddAsync(new AiDialogueTargetWord
            {
                AiDialogueSessionId = session.Id,
                WordMeaningId = meaning.Id,
                IsUsedByStudent = false
            }, cancellationToken);
        }

        // Get AI opening message
        AiChatResponse opening = await _aiService.SendMessageAsync(
            new[]
            {
                new AiChatMessage { Role = "system", Content = systemContext },
                new AiChatMessage { Role = "user", Content = "[START_CONVERSATION]" }
            },
            cancellationToken);

        AiDialogueMessage openingMsg = new()
        {
            AiDialogueSessionId = session.Id,
            Sender = MessageSender.System_AI,
            MessageText = opening.Content
        };
        await _messageRepository.AddAsync(openingMsg, cancellationToken);

        return new StartDialogueSessionResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            OpeningMessage = opening.Content,
            TargetWords = targetWordTexts
        };
    }
}

public class StartDialogueSessionCommandValidator : AbstractValidator<StartDialogueSessionCommand>
{
    public StartDialogueSessionCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty().WithMessage("StudentId is required.");
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(300).WithMessage("Topic must not exceed 300 characters.");
        RuleFor(x => x.TargetWordCount)
            .InclusiveBetween(1, 10).WithMessage("Target word count must be between 1 and 10.");
    }
}
