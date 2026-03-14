namespace OasisWords.Application.Features.Auth.Commands.RegisterInstructor;

public class RegisterInstructorCommand : MediatR.IRequest<RegisterInstructorResponse>
{
    public string FirstName   { get; set; } = string.Empty;
    public string LastName    { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string Password    { get; set; } = string.Empty;
    /// <summary>Eğitmenin uzmanlık alanı / biyografi (opsiyonel).</summary>
    public string? Bio        { get; set; }
}
