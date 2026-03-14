using FluentValidation;
using OasisWords.Application.Features.Auth.Constants;

namespace OasisWords.Application.Features.Auth.Commands.RegisterInstructor;

public class RegisterInstructorCommandValidator : AbstractValidator<RegisterInstructorCommand>
{
    public RegisterInstructorCommandValidator()
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
    }
}
