namespace OasisWords.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommand : MediatR.IRequest<ChangePasswordResponse>
{
    public Guid   UserId          { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}
