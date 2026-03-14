namespace OasisWords.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordResponse
{
    public bool   Success { get; set; }
    public string Message { get; set; } = "Password changed successfully.";
}
