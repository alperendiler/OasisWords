namespace OasisWords.Application.Features.Users.Constants;

public static class UserMessages
{
    public const string UserNotFound         = "User with id '{0}' was not found.";
    public const string FirstNameRequired    = "First name is required.";
    public const string FirstNameTooLong     = "First name must not exceed 100 characters.";
    public const string LastNameRequired     = "Last name is required.";
    public const string LastNameTooLong      = "Last name must not exceed 100 characters.";
    public const string StudentNotFound      = "Student profile not found for this user.";
    public const string DailyGoalRange       = "Daily word goal must be between 1 and 100.";
}
