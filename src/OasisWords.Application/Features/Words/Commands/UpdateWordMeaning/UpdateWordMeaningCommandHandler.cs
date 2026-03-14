using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.UpdateWordMeaning;

public class UpdateWordMeaningCommandHandler : IRequestHandler<UpdateWordMeaningCommand, UpdateWordMeaningResponse>
{
    private readonly IWordMeaningRepository _repo;
    private readonly WordBusinessRules      _rules;

    public UpdateWordMeaningCommandHandler(IWordMeaningRepository repo, WordBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<UpdateWordMeaningResponse> Handle(UpdateWordMeaningCommand request, CancellationToken ct)
    {
        await _rules.WordMeaningShouldExist(request.Id, ct);

        WordMeaning meaning = await _repo.GetAsync(m => m.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        meaning.TranslationText    = request.TranslationText;
        meaning.ExampleSentence    = request.ExampleSentence;
        meaning.ExampleTranslation = request.ExampleTranslation;

        WordMeaning updated = await _repo.UpdateAsync(meaning, ct);
        return new UpdateWordMeaningResponse
        {
            Id = updated.Id, TranslationText = updated.TranslationText, CefrLevel = updated.CefrLevel
        };
    }
}
