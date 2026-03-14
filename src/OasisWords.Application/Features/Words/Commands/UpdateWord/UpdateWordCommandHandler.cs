using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.UpdateWord;

public class UpdateWordCommandHandler : IRequestHandler<UpdateWordCommand, UpdateWordResponse>
{
    private readonly IWordRepository  _repo;
    private readonly WordBusinessRules _rules;

    public UpdateWordCommandHandler(IWordRepository repo, WordBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<UpdateWordResponse> Handle(UpdateWordCommand request, CancellationToken ct)
    {
        await _rules.WordShouldExist(request.Id, ct);

        Word word = await _repo.GetAsync(w => w.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        word.PhoneticSpelling      = request.PhoneticSpelling;
        word.PronunciationAudioUrl = request.PronunciationAudioUrl;

        Word updated = await _repo.UpdateAsync(word, ct);
        return new UpdateWordResponse { Id = updated.Id, Text = updated.Text };
    }
}
