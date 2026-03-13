using FluentValidation;
using MediatR;
using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace OasisWords.Application.Features.AiDialogue.Commands.SendMessage;

public class SendMessageCommand : IRequest<SendMessageResponse>
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public string MessageText { get; set; } = string.Empty;
}

public class SendMessageResponse
{
    public string AiReply { get; set; } = string.Empty;
    public string? CorrectedStudentText { get; set; }
    public bool SessionCompleted { get; set; }
    public int? FinalScore { get; set; }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IAiDialogueSessionRepository _sessionRepository;
    private readonly IAiDialogueMessageRepository _messageRepository;
    private readonly IAiDialogueTargetWordRepository _targetWordRepository;
    private readonly IAiService _aiService;

    public SendMessageCommandHandler(
        IAiDialogueSessionRepository sessionRepository,
        IAiDialogueMessageRepository messageRepository,
        IAiDialogueTargetWordRepository targetWordRepository,
        IAiService aiService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _targetWordRepository = targetWordRepository;
        _aiService = aiService;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        AiDialogueSession? session = await _sessionRepository.GetAsync(
            s => s.Id == request.SessionId && s.StudentId == request.StudentId,
            include: q => q
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .Include(s => s.TargetWords)
                    .ThenInclude(t => t.WordMeaning)
                        .ThenInclude(m => m.Word),
            cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Dialogue session not found.");

        if (session.IsCompleted)
            throw new BusinessException("This dialogue session has already been completed.");

        // Save student message
        AiDialogueMessage studentMsg = new()
        {
            AiDialogueSessionId = session.Id,
            Sender = MessageSender.Student,
            MessageText = request.MessageText
        };
        await _messageRepository.AddAsync(studentMsg, cancellationToken);

        // Check if student used any target words in their message
        foreach (AiDialogueTargetWord targetWord in session.TargetWords.Where(t => !t.IsUsedByStudent))
        {
            string wordText = targetWord.WordMeaning.Word.Text;
            if (request.MessageText.Contains(wordText, StringComparison.OrdinalIgnoreCase))
            {
                targetWord.IsUsedByStudent = true;
                await _targetWordRepository.UpdateAsync(targetWord, cancellationToken);
            }
        }

        // Build conversation history for AI
        List<AiChatMessage> history = new()
        {
            new AiChatMessage { Role = "system", Content = session.SystemPromptContext }
        };

        foreach (AiDialogueMessage msg in session.Messages.OrderBy(m => m.CreatedAt))
        {
            history.Add(new AiChatMessage
            {
                Role = msg.Sender == MessageSender.System_AI ? "assistant" : "user",
                Content = msg.MessageText
            });
        }

        // Get AI reply
        AiChatResponse aiResponse = await _aiService.SendMessageAsync(history, cancellationToken);

        AiDialogueMessage aiMsg = new()
        {
            AiDialogueSessionId = session.Id,
            Sender = MessageSender.System_AI,
            MessageText = aiResponse.Content,
            CorrectedText = aiResponse.CorrectedStudentText
        };
        await _messageRepository.AddAsync(aiMsg, cancellationToken);

        // Complete session if AI provides a score (signals conversation end)
        bool sessionCompleted = aiResponse.Score.HasValue;
        if (sessionCompleted)
        {
            session.IsCompleted = true;
            session.Score = aiResponse.Score!.Value;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        return new SendMessageResponse
        {
            AiReply = aiResponse.Content,
            CorrectedStudentText = aiResponse.CorrectedStudentText,
            SessionCompleted = sessionCompleted,
            FinalScore = sessionCompleted ? aiResponse.Score : null
        };
    }
}

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.StudentId).NotEmpty().WithMessage("StudentId is required.");
        RuleFor(x => x.MessageText)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
