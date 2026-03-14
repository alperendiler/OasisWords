using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.DeleteWord;

public class DeleteWordCommandHandler : IRequestHandler<DeleteWordCommand, DeleteWordResponse>
{
    private readonly IWordRepository  _repo;
    private readonly WordBusinessRules _rules;

    public DeleteWordCommandHandler(IWordRepository repo, WordBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<DeleteWordResponse> Handle(DeleteWordCommand request, CancellationToken ct)
    {
        await _rules.WordShouldExist(request.Id, ct);

        Word word = await _repo.GetAsync(w => w.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        await _repo.DeleteAsync(word, ct);
        return new DeleteWordResponse { Id = request.Id, Deleted = true };
    }
}
