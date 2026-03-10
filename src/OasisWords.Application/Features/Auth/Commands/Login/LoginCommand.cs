using FluentValidation;
using MediatR;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.Login;

// ── Request ──────────────────────────────────────────────────────────────────
public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

// ── Response ─────────────────────────────────────────────────────────────────
public class LoginResponse
{
    public AccessToken? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public bool RequiresTwoFactor { get; set; }
}

// ── Handler ──────────────────────────────────────────────────────────────────
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, request.IpAddress, cancellationToken);
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
