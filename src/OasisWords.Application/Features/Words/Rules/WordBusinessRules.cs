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
        Guid languageId,
        string text,
        CancellationToken cancellationToken = default)
    {
        bool exists = await _wordRepository.AnyAsync(
            w => w.LanguageId == languageId && w.Text.ToLower() == text.ToLower(),
            cancellationToken);

        if (exists)
            throw new BusinessException($"The word '{text}' already exists for this language.");
    }

    public async Task WordMeaningCannotBeDuplicatedForSameLevelAndLanguage(
        Guid wordId,
        Guid translationLanguageId,
        CefrLevel cefrLevel,
        CancellationToken cancellationToken = default)
    {
        bool exists = await _wordMeaningRepository.AnyAsync(
            m => m.WordId == wordId
              && m.TranslationLanguageId == translationLanguageId
              && m.CefrLevel == cefrLevel,
            cancellationToken);

        if (exists)
            throw new BusinessException("A meaning for this word at this CEFR level and language already exists.");
    }

    public async Task LanguageShouldExist(Guid languageId, CancellationToken cancellationToken = default)
    {
        bool exists = await _languageRepository.AnyAsync(l => l.Id == languageId, cancellationToken);
        if (!exists)
            throw new NotFoundException($"Language with id '{languageId}' was not found.");
    }

    public async Task WordShouldExist(Guid wordId, CancellationToken cancellationToken = default)
    {
        bool exists = await _wordRepository.AnyAsync(w => w.Id == wordId, cancellationToken);
        if (!exists)
            throw new NotFoundException($"Word with id '{wordId}' was not found.");
    }
}
