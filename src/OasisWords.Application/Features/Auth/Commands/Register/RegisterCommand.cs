using MediatR;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.Register;

// ── Request ──────────────────────────────────────────────────────────────────
public class RegisterCommand : IRequest<RegisterResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ── Response ─────────────────────────────────────────────────────────────────
public class RegisterResponse
{
    public AccessToken AccessToken { get; set; } = null!;
    public RefreshToken RefreshToken { get; set; } = null!;
}

// ── Handler ──────────────────────────────────────────────────────────────────
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        HashingHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        User user = new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        RegisterResponse response = await _authService.RegisterAsync(user, cancellationToken);
        return response;
    }
}
