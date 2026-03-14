namespace OasisWords.Application.Features.Auth.Commands.Login;

public class LoginCommand : MediatR.IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}
