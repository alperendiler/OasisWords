namespace OasisWords.Application.Features.Auth.Constants;

public static class AuthMessages
{
    // Registration
    public const string EmailAlreadyExists       = "A user with this email address already exists.";
    public const string EmailRequired            = "Email address is required.";
    public const string EmailInvalid             = "A valid email address is required.";
    public const string PasswordRequired         = "Password is required.";
    public const string PasswordTooShort         = "Password must be at least 8 characters.";
    public const string PasswordMustHaveUpper    = "Password must contain at least one uppercase letter.";
    public const string PasswordMustHaveDigit    = "Password must contain at least one digit.";
    public const string PasswordMustHaveSpecial  = "Password must contain at least one special character.";
    public const string FirstNameRequired        = "First name is required.";
    public const string FirstNameTooLong         = "First name must not exceed 100 characters.";
    public const string LastNameRequired         = "Last name is required.";
    public const string LastNameTooLong          = "Last name must not exceed 100 characters.";

    // Login / Auth
    public const string InvalidCredentials       = "Email or password is incorrect.";
    public const string AccountDeactivated       = "This account has been deactivated.";
    public const string UserNotFound             = "Email or password is incorrect.";

    // Token
    public const string RefreshTokenNotFound     = "Refresh token not found.";
    public const string RefreshTokenInactive     = "Refresh token is no longer active.";
    public const string RefreshTokenRequired     = "Refresh token is required.";

    // Student registration
    public const string DailyWordGoalRange       = "Daily word goal must be between 1 and 100.";
    public const string NativeLanguageRequired   = "Native language is required.";
    public const string TargetLanguageRequired   = "Target language is required.";
    public const string CefrLevelInvalid         = "CEFR level must be a valid value (A1–C2).";
    public const string SameNativeAndTarget      = "Native and target languages cannot be the same.";

    // Password change
    public const string CurrentPasswordRequired  = "Current password is required.";
    public const string NewPasswordRequired      = "New password is required.";
    public const string CurrentPasswordWrong     = "Current password is incorrect.";
    public const string NewPasswordSameAsOld     = "New password must differ from the current password.";
}
