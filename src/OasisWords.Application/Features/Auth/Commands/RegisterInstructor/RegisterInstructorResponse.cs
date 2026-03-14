using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.RegisterInstructor;

public class RegisterInstructorResponse
{
    public Guid        UserId       { get; set; }
    public AccessToken AccessToken  { get; set; } = null!;
    public string      RefreshToken { get; set; } = string.Empty;
}
