using MediatR;
using OasisWords.Application.Services.AuthService;
using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.Hashing;

namespace OasisWords.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        HashingHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

        User user = new()
        {
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt    = DateTime.UtcNow
        };

        return await _authService.RegisterAsync(user, ct);
    }
}
