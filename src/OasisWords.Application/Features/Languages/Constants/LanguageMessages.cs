namespace OasisWords.Application.Features.Languages.Constants;

public static class LanguageMessages
{
    public const string NameRequired        = "Language name is required.";
    public const string NameTooLong         = "Language name must not exceed 100 characters.";
    public const string CodeRequired        = "Language code is required (e.g. 'en', 'tr').";
    public const string CodeTooLong         = "Language code must not exceed 10 characters.";
    public const string CodeAlreadyExists   = "A language with the code '{0}' already exists.";
    public const string LanguageNotFound    = "Language with id '{0}' was not found.";
    public const string FlagUrlTooLong      = "Flag image URL must not exceed 512 characters.";
    public const string CannotDeleteInUse   = "Language cannot be deleted because it is referenced by existing words or profiles.";
}
