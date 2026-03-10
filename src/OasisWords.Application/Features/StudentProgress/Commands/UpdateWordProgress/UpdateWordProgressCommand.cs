using FluentValidation;
using MediatR;
using OasisWords.Application.Services.StudentProgressService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.StudentProgress.Commands.UpdateWordProgress;

public class UpdateWordProgressCommand : IRequest<UpdateWordProgressResponse>
{
    public Guid StudentId { get; set; }
    public Guid WordMeaningId { get; set; }
    public bool IsCorrect { get; set; }
}

public class UpdateWordProgressResponse
{
    public Guid WordMeaningId { get; set; }
    public WordLearningStatus NewStatus { get; set; }
    public DateTime NextReviewDate { get; set; }
    public int ConsecutiveCorrectAnswers { get; set; }
}

public class UpdateWordProgressCommandHandler : IRequestHandler<UpdateWordProgressCommand, UpdateWordProgressResponse>
{
    private readonly IStudentWordProgressRepository _progressRepository;

    public UpdateWordProgressCommandHandler(IStudentWordProgressRepository progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public async Task<UpdateWordProgressResponse> Handle(
        UpdateWordProgressCommand request,
        CancellationToken cancellationToken)
    {
        StudentWordProgress? progress = await _progressRepository.GetAsync(
            p => p.StudentId == request.StudentId && p.WordMeaningId == request.WordMeaningId,
            cancellationToken: cancellationToken);

        DateTime now = DateTime.UtcNow;

        if (progress is null)
        {
            // First time seeing this word
            progress = new StudentWordProgress
            {
                StudentId = request.StudentId,
                WordMeaningId = request.WordMeaningId,
                Status = WordLearningStatus.New,
                NextReviewDate = now,
                ConsecutiveCorrectAnswers = 0,
                TotalIncorrectAnswers = 0
            };
        }

        if (request.IsCorrect)
        {
            progress.ConsecutiveCorrectAnswers++;
            progress.NextReviewDate = CalculateNextReviewDate(progress.ConsecutiveCorrectAnswers, now);
            progress.Status = DetermineStatus(progress.ConsecutiveCorrectAnswers);
        }
        else
        {
            progress.TotalIncorrectAnswers++;
            progress.ConsecutiveCorrectAnswers = 0;
            progress.Status = WordLearningStatus.Learning;
            progress.NextReviewDate = now.Date.AddDays(1);
        }

        progress.LastReviewedAt = now;

        if (progress.Id == Guid.Empty)
            await _progressRepository.AddAsync(progress, cancellationToken);
        else
            await _progressRepository.UpdateAsync(progress, cancellationToken);

        return new UpdateWordProgressResponse
        {
            WordMeaningId = progress.WordMeaningId,
            NewStatus = progress.Status,
            NextReviewDate = progress.NextReviewDate,
            ConsecutiveCorrectAnswers = progress.ConsecutiveCorrectAnswers
        };
    }

    // SRS interval ladder: 1 → 3 → 7 → 14 → 30 → 90 days
    private static DateTime CalculateNextReviewDate(int consecutiveCorrect, DateTime now)
    {
        int daysToAdd = consecutiveCorrect switch
        {
            1 => 1,
            2 => 3,
            3 => 7,
            4 => 14,
            5 => 30,
            _ => 90
        };
        return now.Date.AddDays(daysToAdd);
    }

    private static WordLearningStatus DetermineStatus(int consecutiveCorrect)
    {
        return consecutiveCorrect switch
        {
            <= 1 => WordLearningStatus.Learning,
            <= 3 => WordLearningStatus.Reviewing,
            _ => WordLearningStatus.Mastered
        };
    }
}

public class UpdateWordProgressCommandValidator : AbstractValidator<UpdateWordProgressCommand>
{
    public UpdateWordProgressCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty().WithMessage("StudentId is required.");
        RuleFor(x => x.WordMeaningId).NotEmpty().WithMessage("WordMeaningId is required.");
    }
}
