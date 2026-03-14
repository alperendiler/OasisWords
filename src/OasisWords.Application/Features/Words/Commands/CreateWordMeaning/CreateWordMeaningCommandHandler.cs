using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.CreateWordMeaning;

public class CreateWordMeaningCommandHandler : IRequestHandler<CreateWordMeaningCommand, CreateWordMeaningResponse>
{
    private readonly IWordMeaningRepository _meaningRepository;
    private readonly WordBusinessRules _rules;

    public CreateWordMeaningCommandHandler(IWordMeaningRepository meaningRepository, WordBusinessRules rules)
    {
        _meaningRepository = meaningRepository;
        _rules = rules;
    }

    public async Task<CreateWordMeaningResponse> Handle(CreateWordMeaningCommand request, CancellationToken ct)
    {
        await _rules.WordShouldExist(request.WordId, ct);
        await _rules.LanguageShouldExist(request.TranslationLanguageId, ct);
        await _rules.WordMeaningCannotBeDuplicatedForSameLevelAndLanguage(
            request.WordId, request.TranslationLanguageId, request.CefrLevel, ct);

        WordMeaning meaning = new()
        {
            WordId = request.WordId,
            TranslationLanguageId = request.TranslationLanguageId,
            CefrLevel = request.CefrLevel,
            TranslationText = request.TranslationText,
            ExampleSentence = request.ExampleSentence,
            ExampleTranslation = request.ExampleTranslation
        };

        WordMeaning created = await _meaningRepository.AddAsync(meaning, ct);
        return new CreateWordMeaningResponse
        {
            Id = created.Id, WordId = created.WordId,
            CefrLevel = created.CefrLevel, TranslationText = created.TranslationText
        };
    }
}
