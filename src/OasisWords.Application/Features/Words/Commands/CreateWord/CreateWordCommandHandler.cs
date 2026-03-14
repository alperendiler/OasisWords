using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.CreateWord;

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, CreateWordResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly WordBusinessRules _rules;

    public CreateWordCommandHandler(IWordRepository wordRepository, WordBusinessRules rules)
    {
        _wordRepository = wordRepository;
        _rules = rules;
    }

    public async Task<CreateWordResponse> Handle(CreateWordCommand request, CancellationToken ct)
    {
        await _rules.LanguageShouldExist(request.LanguageId, ct);
        await _rules.WordCannotBeDuplicatedForSameLanguage(request.LanguageId, request.Text, ct);

        Word word = new()
        {
            LanguageId = request.LanguageId,
            Text = request.Text.Trim().ToLower(),
            PhoneticSpelling = request.PhoneticSpelling,
            PronunciationAudioUrl = request.PronunciationAudioUrl
        };

        Word created = await _wordRepository.AddAsync(word, ct);
        return new CreateWordResponse { Id = created.Id, Text = created.Text, LanguageId = created.LanguageId };
    }
}
