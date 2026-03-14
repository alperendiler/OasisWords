using FluentValidation;
using OasisWords.Application.Features.Users.Constants;

namespace OasisWords.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(UserMessages.FirstNameRequired)
                                 .MaximumLength(100).WithMessage(UserMessages.FirstNameTooLong);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(UserMessages.LastNameRequired)
                                .MaximumLength(100).WithMessage(UserMessages.LastNameTooLong);
        RuleFor(x => x.DailyWordGoal)
            .InclusiveBetween(1, 100).WithMessage(UserMessages.DailyGoalRange)
            .When(x => x.DailyWordGoal.HasValue);
    }
}
