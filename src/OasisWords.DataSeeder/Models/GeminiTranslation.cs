namespace OasisWords.DataSeeder.Models;

/// <summary>
/// Represents one translated word as returned by Gemini in JSON array format.
/// </summary>
public class GeminiTranslation
{
    public string Word { get; set; } = string.Empty;
    public string Cefr { get; set; } = string.Empty;

    /// <summary>Contextual Turkish translation (not a raw dictionary lookup)</summary>
    public string TranslationTr { get; set; } = string.Empty;

    /// <summary>Example English sentence using the word in context</summary>
    public string ExampleSentence { get; set; } = string.Empty;

    /// <summary>Turkish translation of the example sentence</summary>
    public string ExampleTranslation { get; set; } = string.Empty;
}

/// <summary>API request/response shapes for the OasisWords REST API</summary>
public class CreateWordRequest
{
    public Guid LanguageId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class CreateWordResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class CreateWordMeaningRequest
{
    public Guid WordId { get; set; }
    public Guid TranslationLanguageId { get; set; }
    public int CefrLevel { get; set; }
    public string TranslationText { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ExampleTranslation { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public AccessTokenDto? AccessToken { get; set; }
}

public class AccessTokenDto
{
    public string Token { get; set; } = string.Empty;
}
