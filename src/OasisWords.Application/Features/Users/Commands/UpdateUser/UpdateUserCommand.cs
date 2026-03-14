namespace OasisWords.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : MediatR.IRequest<UpdateUserResponse>
{
    public Guid   UserId    { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    /// <summary>Öğrenciyse günlük kelime hedefi (null = değiştirme).</summary>
    public int? DailyWordGoal { get; set; }
}
