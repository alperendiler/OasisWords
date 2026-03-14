using OasisWords.Core.Security.Entities;
using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.Register;

public class RegisterResponse
{
    public AccessToken AccessToken { get; set; } = null!;
    public RefreshToken RefreshToken { get; set; } = null!;
}
