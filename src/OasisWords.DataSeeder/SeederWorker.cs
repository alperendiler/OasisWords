using CsvHelper;
using CsvHelper.Configuration;
using OasisWords.DataSeeder.Models;
using OasisWords.DataSeeder.Services;
using System.Globalization;

namespace OasisWords.DataSeeder;

/// <summary>
/// Hosted worker that:
///  1. Reads oxford_words.csv
///  2. Sends batches of words to Gemini for contextual TR translation
///  3. Pushes each word + meaning to the OasisWords API
///  Then exits (RunOnce pattern — run as a job, not a daemon)
/// </summary>
public class SeederWorker : BackgroundService
{
    private readonly ILogger<SeederWorker> _logger;
    private readonly SeederSettings _settings;
    private readonly GeminiTranslationService _geminiService;
    private readonly OasisWordsApiClient _apiClient;
    private readonly IHostApplicationLifetime _lifetime;
    // YENİ EKLENEN: Kaç kelime işlediğimizi kaydedeceğimiz ufak bir text dosyası
    private readonly string _checkpointFile = "Data/seeder_checkpoint.txt";

    // CEFR string → int mapping (matches CefrLevel enum in Domain)
    private static readonly Dictionary<string, int> CefrMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a1"] = 1, ["a2"] = 2, ["b1"] = 3,
        ["b2"] = 4, ["c1"] = 5, ["c2"] = 6
    };

    public SeederWorker(
        ILogger<SeederWorker> logger,
        SeederSettings settings,
        GeminiTranslationService geminiService,
        OasisWordsApiClient apiClient,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _settings = settings;
        _geminiService = geminiService;
        _apiClient = apiClient;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("OasisWords DataSeeder starting…");
            _logger.LogInformation("Waiting 3 seconds for Web API to boot up...");
            await Task.Delay(3000, stoppingToken);
            // Step 1 — Authenticate
            await _apiClient.AuthenticateAsync(stoppingToken);

            // Step 2 — Read CSV
            List<EnglishCefrWord> allWords = ReadCsv(_settings.CsvFilePath);
            _logger.LogInformation("Loaded {Count} words from CSV.", allWords.Count);

            int startIndex = 0;
            if (File.Exists(_checkpointFile))
            {
                string savedIndex = File.ReadAllText(_checkpointFile);
                if (int.TryParse(savedIndex, out int parsedIndex))
                {
                    startIndex = parsedIndex;
                    _logger.LogInformation(">>> CHECKPOINT BULUNDU: İşleme {StartIndex}. kelimeden devam edilecek. <<<", startIndex);
                }
            }

            // Eğer startIndex varsa, allWords listesindeki ilk N kelimeyi (zaten işlenenleri) listeden at
            if (startIndex > 0)
            {
                allWords = allWords.Skip(startIndex).ToList();
            }
            // Step 3 — Process in batches
            int processed = 0;
            int skipped = 0;
            int failed = 0;

            foreach (IEnumerable<EnglishCefrWord> batch in allWords.Chunk(_settings.BatchSize))
            {
                stoppingToken.ThrowIfCancellationRequested();

                List<EnglishCefrWord> batchList = batch.ToList();
                processed += batchList.Count;
                File.WriteAllText(_checkpointFile, processed.ToString()); // Her 10 kelimede bir sayıyı dosyaya yazar

                // Rate-limit safety delay
                if (_settings.DelayBetweenBatchesMs > 0)
                    await Task.Delay(_settings.DelayBetweenBatchesMs, stoppingToken);

                _logger.LogInformation(
                    "Processing batch {From}–{To} of {Total}…",
                    processed + 1, processed + batchList.Count, allWords.Count);

                // Step 3a — Get Gemini translations
                List<GeminiTranslation> translations = await _geminiService
                    .TranslateBatchAsync(batchList, stoppingToken);

                if (translations.Count == 0)
                {
                    _logger.LogWarning("Gemini returned 0 translations for this batch, skipping.");
                    skipped += batchList.Count;
                    processed += batchList.Count;
                    continue;
                }

                // Build a lookup by word text (case-insensitive)
                Dictionary<string, GeminiTranslation> translationLookup = translations
                      .DistinctBy(t => t.Word, StringComparer.OrdinalIgnoreCase)
                      .ToDictionary(t => t.Word, StringComparer.OrdinalIgnoreCase);

                // Step 3b — Push each word to API
                foreach (EnglishCefrWord record in batchList)
                {
                    try
                    {
                        if (!translationLookup.TryGetValue(record.Word, out GeminiTranslation? tr))
                        {
                            _logger.LogWarning("No translation returned for '{Word}', skipping.", record.Word);
                            skipped++;
                            continue;
                        }

                        // Create the word
                        Guid? wordId = await _apiClient.CreateWordAsync(
                            record.Word.Trim().ToLower(),
                            _settings.EnglishLanguageId,
                            stoppingToken);

                        if (wordId is null)
                        {
                            // Already exists — nothing more to do for this word
                            skipped++;
                            continue;
                        }

                        if (!CefrMap.TryGetValue(record.Cefr, out int cefrInt))
                        {
                            _logger.LogWarning("Unknown CEFR level '{Cefr}' for '{Word}', defaulting to A1.", record.Cefr, record.Word);
                            cefrInt = 1;
                        }

                        // Create the Turkish meaning
                        await _apiClient.CreateWordMeaningAsync(
                            wordId.Value,
                            _settings.TurkishLanguageId,
                            cefrInt,
                            tr.TranslationTr,
                            tr.ExampleSentence,
                            tr.ExampleTranslation,
                            stoppingToken);

                        processed++;
                        _logger.LogDebug("Seeded: {Word} → {Translation}", record.Word, tr.TranslationTr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to seed word '{Word}'.", record.Word);
                        failed++;
                    }
                }

                // Rate-limit safety delay
                if (_settings.DelayBetweenBatchesMs > 0)
                    await Task.Delay(_settings.DelayBetweenBatchesMs, stoppingToken);
            }

            _logger.LogInformation(
                "Seeding complete. Processed={Processed} Skipped={Skipped} Failed={Failed}",
                processed, skipped, failed);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Seeder cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Seeder encountered a fatal error.");
            Console.WriteLine("Hata alındı! Ekranın kapanmaması için Enter'a basana kadar bekleyecek...");
            Console.ReadLine(); // EKRANI DONDUR
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private static List<EnglishCefrWord> ReadCsv(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV file not found: {path}");

        using StreamReader reader = new(path);
        using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null
        });

        return csv.GetRecords<EnglishCefrWord>().ToList();
    }
}
