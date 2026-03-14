namespace OasisWords.Application.Features.Words.Constants;

public static class WordMessages
{
    // Word
    public const string WordRequired            = "Word text is required.";
    public const string WordTooLong             = "Word text must not exceed 200 characters.";
    public const string WordAlreadyExists       = "The word '{0}' already exists for this language.";
    public const string WordNotFound            = "Word with id '{0}' was not found.";
    public const string LanguageRequired        = "Language is required.";
    public const string AudioUrlTooLong         = "Audio URL must not exceed 512 characters.";
    public const string PhoneticTooLong         = "Phonetic spelling must not exceed 200 characters.";

    // WordMeaning
    public const string MeaningRequired         = "Translation text is required.";
    public const string MeaningTooLong          = "Translation must not exceed 500 characters.";
    public const string MeaningAlreadyExists    = "A meaning for this word at this CEFR level and language already exists.";
    public const string MeaningNotFound         = "Word meaning with id '{0}' was not found.";
    public const string CefrLevelInvalid        = "CEFR level must be a valid value (A1–C2).";
    public const string ExampleSentenceTooLong  = "Example sentence must not exceed 1000 characters.";
    public const string ExampleTranslationTooLong = "Example translation must not exceed 1000 characters.";
    public const string TranslationLanguageRequired = "Translation language is required.";
    public const string WordIdRequired          = "Word is required.";
}
