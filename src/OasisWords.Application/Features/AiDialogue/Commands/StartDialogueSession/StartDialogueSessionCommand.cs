using FluentValidation;
using MediatR;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace OasisWords.Application.Features.AiDialogue.Commands.StartDialogueSession;

public class StartDialogueSessionCommand : IRequest<StartDialogueSessionResponse>
{
    public Guid StudentId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int TargetWordCount { get; set; } = 5;

    /// <summary>
    /// Student's CEFR level — used to tailor the AI's language complexity.
    /// If null the handler will attempt to read it from the student's profile.
    /// </summary>
    public CefrLevel? StudentLevel { get; set; }
}

public class StartDialogueSessionResponse
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string OpeningMessage { get; set; } = string.Empty;
    public IList<string> TargetWords { get; set; } = new List<string>();
    public string SystemPromptPreview { get; set; } = string.Empty;
}

public class StartDialogueSessionCommandHandler : IRequestHandler<StartDialogueSessionCommand, StartDialogueSessionResponse>
{
    private readonly IAiDialogueSessionRepository _sessionRepository;
    private readonly IAiDialogueMessageRepository _messageRepository;
    private readonly IAiDialogueTargetWordRepository _targetWordRepository;
    private readonly IStudentWordProgressRepository _progressRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IWordMeaningRepository _wordMeaningRepository;
    private readonly IAiService _aiService;

    public StartDialogueSessionCommandHandler(
        IAiDialogueSessionRepository sessionRepository,
        IAiDialogueMessageRepository messageRepository,
        IAiDialogueTargetWordRepository targetWordRepository,
        IStudentWordProgressRepository progressRepository,
        IStudentRepository studentRepository,
        IWordMeaningRepository wordMeaningRepository,
        IAiService aiService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _targetWordRepository = targetWordRepository;
        _progressRepository = progressRepository;
        _studentRepository = studentRepository;
        _wordMeaningRepository = wordMeaningRepository;
        _aiService = aiService;
    }

    public async Task<StartDialogueSessionResponse> Handle(
        StartDialogueSessionCommand request,
        CancellationToken cancellationToken)
    {
        // ── Load student profile to get native language + CEFR level ──────────
        Student student = await _studentRepository.GetAsync(
            s => s.Id == request.StudentId,
            include: q => q
                .Include(s => s.LanguageProfiles)
                    .ThenInclude(lp => lp.NativeLanguage)
                .Include(s => s.LanguageProfiles)
                    .ThenInclude(lp => lp.TargetLanguage),
            enableTracking: false,
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Student not found.");

        StudentLanguageProfile? profile = student.LanguageProfiles.FirstOrDefault();
        CefrLevel cefrLevel = request.StudentLevel
            ?? profile?.TargetCefrLevel
            ?? CefrLevel.B1;

        string nativeLanguageName = profile?.NativeLanguage.Name ?? "Turkish";
        string targetLanguageName = profile?.TargetLanguage.Name ?? "English";

        // ── Select recently-learned words to reinforce in dialogue ────────────
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

        // ── Build dynamic system prompt ───────────────────────────────────────
        string systemPrompt = BuildSystemPrompt(
            topic: request.Topic,
            targetWords: targetWordTexts,
            cefrLevel: cefrLevel,
            nativeLanguage: nativeLanguageName,
            targetLanguage: targetLanguageName);

        // ── Create session entity ─────────────────────────────────────────────
        AiDialogueSession session = new()
        {
            StudentId = request.StudentId,
            Topic = request.Topic,
            SystemPromptContext = systemPrompt,
            IsCompleted = false,
            Score = 0
        };
        session = await _sessionRepository.AddAsync(session, cancellationToken);

        // ── Save target words ─────────────────────────────────────────────────
        foreach (WordMeaning meaning in targetMeanings)
        {
            await _targetWordRepository.AddAsync(new AiDialogueTargetWord
            {
                AiDialogueSessionId = session.Id,
                WordMeaningId = meaning.Id,
                IsUsedByStudent = false
            }, cancellationToken);
        }

        // ── Get AI opening message ────────────────────────────────────────────
        AiChatResponse opening = await _aiService.SendMessageAsync(
            new[]
            {
                new AiChatMessage { Role = "system", Content = systemPrompt },
                new AiChatMessage { Role = "user",   Content = "[START]" }
            },
            cancellationToken);

        await _messageRepository.AddAsync(new AiDialogueMessage
        {
            AiDialogueSessionId = session.Id,
            Sender = MessageSender.System_AI,
            MessageText = opening.Content
        }, cancellationToken);

        return new StartDialogueSessionResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            OpeningMessage = opening.Content,
            TargetWords = targetWordTexts,
            SystemPromptPreview = systemPrompt[..Math.Min(200, systemPrompt.Length)] + "…"
        };
    }

    // ── Dynamic System Prompt Builder ─────────────────────────────────────────
    private static string BuildSystemPrompt(
        string topic,
        IEnumerable<string> targetWords,
        CefrLevel cefrLevel,
        string nativeLanguage,
        string targetLanguage)
    {
        string cefrLabel = cefrLevel.ToString(); // "A1", "B2", etc.
        string wordList = string.Join(", ", targetWords.Select(w => $"\"{w}\""));

        string complexityGuide = cefrLevel switch
        {
            CefrLevel.A1 => "Use very simple sentences (subject + verb + object). Avoid idioms, passive voice, and complex tenses. Vocabulary should be basic everyday words.",
            CefrLevel.A2 => "Use simple, clear sentences. Stick to present simple, past simple, and basic future forms. Avoid idioms.",
            CefrLevel.B1 => "Use moderately complex sentences. Present perfect, conditionals, and common phrasal verbs are acceptable. Avoid rare idioms.",
            CefrLevel.B2 => "Use natural conversational English. Complex sentence structures, conditionals, and idiomatic expressions are welcome.",
            CefrLevel.C1 => "Use sophisticated, nuanced English. Idiomatic language, complex grammar, and abstract vocabulary are encouraged.",
            CefrLevel.C2 => "Use fully native-level English in all its richness — complex idioms, subtle nuances, literary references if appropriate.",
            _ => "Use natural conversational English."
        };

        return $"""
            You are a friendly and patient {targetLanguage} conversation teacher.
            The student's native language is {nativeLanguage} and their {targetLanguage} level is {cefrLabel}.

            LANGUAGE COMPLEXITY GUIDE FOR {cefrLabel}:
            {complexityGuide}

            CONVERSATION TOPIC: {topic}

            TARGET VOCABULARY WORDS (you MUST use each of these naturally during the conversation, and gently encourage the student to use them too):
            {wordList}

            YOUR BEHAVIOUR RULES:
            1. Start the conversation with a warm greeting and an open question related to the topic.
            2. Keep your responses SHORT (2–4 sentences) so the student has room to reply.
            3. When the student makes a grammar or vocabulary error, correct it gently inline using the format: [CORRECTION: correct version here].
            4. Naturally weave the target vocabulary words into your own sentences first to model their usage.
            5. After 6–8 student turns, wrap up the conversation warmly and give a score from 0–100 using the format [SCORE:XX], considering grammar accuracy (50%) and target word usage (50%).
            6. Never break character. Do not discuss anything unrelated to {topic} and language learning.
            """;
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
