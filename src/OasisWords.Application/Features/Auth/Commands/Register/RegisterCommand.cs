namespace OasisWords.Application.Features.Auth.Commands.Register;

public class RegisterCommand : MediatR.IRequest<RegisterResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
}
