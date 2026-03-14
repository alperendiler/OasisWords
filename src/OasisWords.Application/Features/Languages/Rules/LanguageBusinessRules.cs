using OasisWords.Application.Features.Languages.Constants;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;

namespace OasisWords.Application.Features.Languages.Rules;

public class LanguageBusinessRules
{
    private readonly ILanguageRepository _languageRepository;

    public LanguageBusinessRules(ILanguageRepository languageRepository)
        => _languageRepository = languageRepository;

    public async Task LanguageCodeCannotBeDuplicated(string code, CancellationToken ct = default)
    {
        bool exists = await _languageRepository.AnyAsync(
            l => l.Code.ToLower() == code.ToLower(), ct);
        if (exists)
            throw new BusinessException(string.Format(LanguageMessages.CodeAlreadyExists, code));
    }

    public async Task LanguageShouldExist(Guid id, CancellationToken ct = default)
    {
        bool exists = await _languageRepository.AnyAsync(l => l.Id == id, ct);
        if (!exists)
            throw new NotFoundException(string.Format(LanguageMessages.LanguageNotFound, id));
    }
}
