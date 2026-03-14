using OasisWords.Application.Features.Words.Constants;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Rules;

public class WordBusinessRules
{
    private readonly IWordRepository _wordRepository;
    private readonly IWordMeaningRepository _wordMeaningRepository;
    private readonly ILanguageRepository _languageRepository;

    public WordBusinessRules(
        IWordRepository wordRepository,
        IWordMeaningRepository wordMeaningRepository,
        ILanguageRepository languageRepository)
    {
        _wordRepository = wordRepository;
        _wordMeaningRepository = wordMeaningRepository;
        _languageRepository = languageRepository;
    }

    public async Task WordCannotBeDuplicatedForSameLanguage(
        Guid languageId, string text, CancellationToken ct = default)
    {
        bool exists = await _wordRepository.AnyAsync(
            w => w.LanguageId == languageId && w.Text.ToLower() == text.ToLower(), ct);
        if (exists)
            throw new BusinessException(string.Format(WordMessages.WordAlreadyExists, text));
    }

    public async Task WordMeaningCannotBeDuplicatedForSameLevelAndLanguage(
        Guid wordId, Guid translationLanguageId, CefrLevel cefrLevel, CancellationToken ct = default)
    {
        bool exists = await _wordMeaningRepository.AnyAsync(
            m => m.WordId == wordId
              && m.TranslationLanguageId == translationLanguageId
              && m.CefrLevel == cefrLevel, ct);
        if (exists)
            throw new BusinessException(WordMessages.MeaningAlreadyExists);
    }

    public async Task LanguageShouldExist(Guid languageId, CancellationToken ct = default)
    {
        bool exists = await _languageRepository.AnyAsync(l => l.Id == languageId, ct);
        if (!exists)
            throw new NotFoundException(string.Format(WordMessages.WordNotFound, languageId));
    }

    public async Task WordShouldExist(Guid wordId, CancellationToken ct = default)
    {
        bool exists = await _wordRepository.AnyAsync(w => w.Id == wordId, ct);
        if (!exists)
            throw new NotFoundException(string.Format(WordMessages.WordNotFound, wordId));
    }

    public async Task WordMeaningShouldExist(Guid meaningId, CancellationToken ct = default)
    {
        bool exists = await _wordMeaningRepository.AnyAsync(m => m.Id == meaningId, ct);
        if (!exists)
            throw new NotFoundException(string.Format(WordMessages.MeaningNotFound, meaningId));
    }
}
