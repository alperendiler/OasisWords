using OasisWords.DataSeeder.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OasisWords.DataSeeder.Services;

/// <summary>
/// Thin HTTP client that talks directly to the OasisWords REST API.
/// Authenticates once and reuses the JWT for all subsequent calls.
/// </summary>
public class OasisWordsApiClient
{
    private readonly HttpClient _http;
    private readonly SeederSettings _settings;
    private readonly ILogger<OasisWordsApiClient> _logger;
    private string? _token;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OasisWordsApiClient(
        HttpClient http,
        SeederSettings settings,
        ILogger<OasisWordsApiClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/");
        _settings = settings;
        _logger = logger;
    }

    public async Task AuthenticateAsync(CancellationToken ct = default)
    {
        try
        {
            var req = new LoginRequest
            {
                Email = _settings.AdminEmail,
                Password = _settings.AdminPassword
            };

            HttpResponseMessage response = await _http.PostAsJsonAsync("api/auth/login", req, ct);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("\n=== API GİRİŞ HATASI ===");
                _logger.LogError("Status: {StatusCode}", response.StatusCode);
                _logger.LogError("Detay: {Error}", error);
                _logger.LogError("========================\n");
            }

            response.EnsureSuccessStatusCode();

            LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts, ct);
            _token = login?.AccessToken?.Token
                ?? throw new InvalidOperationException("Login returned no access token.");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            _logger.LogInformation("Authenticated with OasisWords API as admin.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "\n!!! WEB API'YE BAĞLANILAMADI !!! Web API projesinin ({Url}) çalıştığından emin misin?", _settings.ApiBaseUrl);
            throw;
        }
    }
    /// <summary>Creates a word; returns null if it already exists (409 Conflict).</summary>
    public async Task<Guid?> CreateWordAsync(string text, Guid languageId, CancellationToken ct = default)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var req = new CreateWordRequest { LanguageId = languageId, Text = text };

        HttpResponseMessage response = await _http.PostAsJsonAsync("api/words", req, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict ||
            response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Word already exists — read its id from a GET instead
            _logger.LogDebug("Word '{Word}' already exists, skipping create.", text);
            return null;
        }

        response.EnsureSuccessStatusCode();
        CreateWordResponse? result = await response.Content.ReadFromJsonAsync<CreateWordResponse>(JsonOpts, ct);
        return result?.Id;
    }

    public async Task CreateWordMeaningAsync(

        Guid wordId,
        Guid translationLanguageId,
        int cefrLevel,
        string translationText,
        string? exampleSentence,
        string? exampleTranslation,
        CancellationToken ct = default)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var req = new CreateWordMeaningRequest
        {
            WordId = wordId,
            TranslationLanguageId = translationLanguageId,
            CefrLevel = cefrLevel,
            TranslationText = translationText,
            ExampleSentence = exampleSentence,
            ExampleTranslation = exampleTranslation
        };

        HttpResponseMessage response = await _http.PostAsJsonAsync("api/words/meanings", req, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogDebug("Meaning for wordId={WordId} cefr={Cefr} already exists.", wordId, cefrLevel);
            return;
        }

        response.EnsureSuccessStatusCode();
    }
}
