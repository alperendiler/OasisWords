using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Auth.Commands.RegisterStudent;

public class RegisterStudentCommand : MediatR.IRequest<RegisterStudentResponse>
{
    public string FirstName        { get; set; } = string.Empty;
    public string LastName         { get; set; } = string.Empty;
    public string Email            { get; set; } = string.Empty;
    public string Password         { get; set; } = string.Empty;
    public int    DailyWordGoal    { get; set; } = 10;
    public Guid   NativeLanguageId { get; set; }
    public Guid   TargetLanguageId { get; set; }
    public CefrLevel TargetCefrLevel { get; set; } = CefrLevel.A1;
}
