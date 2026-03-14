namespace OasisWords.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserResponse
{
    public Guid   UserId    { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
}
