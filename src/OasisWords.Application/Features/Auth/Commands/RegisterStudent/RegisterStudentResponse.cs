using OasisWords.Core.Security.JWT;

namespace OasisWords.Application.Features.Auth.Commands.RegisterStudent;

public class RegisterStudentResponse
{
    public Guid        UserId       { get; set; }
    public Guid        StudentId    { get; set; }
    public AccessToken AccessToken  { get; set; } = null!;
    public string      RefreshToken { get; set; } = string.Empty;
}
