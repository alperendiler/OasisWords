using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Auth.Commands.RegisterStudent;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(AuthMessages.FirstNameRequired)
                                 .MaximumLength(100).WithMessage(AuthMessages.FirstNameTooLong);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(AuthMessages.LastNameRequired)
                                .MaximumLength(100).WithMessage(AuthMessages.LastNameTooLong);
        RuleFor(x => x.Email).NotEmpty().WithMessage(AuthMessages.EmailRequired)
                             .EmailAddress().WithMessage(AuthMessages.EmailInvalid);
        RuleFor(x => x.Password).NotEmpty().WithMessage(AuthMessages.PasswordRequired)
                                .MinimumLength(8).WithMessage(AuthMessages.PasswordTooShort)
                                .Matches("[A-Z]").WithMessage(AuthMessages.PasswordMustHaveUpper)
                                .Matches("[0-9]").WithMessage(AuthMessages.PasswordMustHaveDigit)
                                .Matches("[^a-zA-Z0-9]").WithMessage(AuthMessages.PasswordMustHaveSpecial);
        RuleFor(x => x.DailyWordGoal).InclusiveBetween(1, 100).WithMessage(AuthMessages.DailyWordGoalRange);
        RuleFor(x => x.NativeLanguageId).NotEmpty().WithMessage(AuthMessages.NativeLanguageRequired);
        RuleFor(x => x.TargetLanguageId).NotEmpty().WithMessage(AuthMessages.TargetLanguageRequired);
        RuleFor(x => x.TargetCefrLevel).IsInEnum().WithMessage(AuthMessages.CefrLevelInvalid);
        RuleFor(x => x).Must(x => x.NativeLanguageId != x.TargetLanguageId)
                       .WithMessage(AuthMessages.SameNativeAndTarget)
                       .When(x => x.NativeLanguageId != Guid.Empty && x.TargetLanguageId != Guid.Empty);
    }
}
