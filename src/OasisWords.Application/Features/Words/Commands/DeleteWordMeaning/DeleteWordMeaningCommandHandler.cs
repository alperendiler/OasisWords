using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Commands.DeleteWordMeaning;

public class DeleteWordMeaningCommandHandler : IRequestHandler<DeleteWordMeaningCommand, DeleteWordMeaningResponse>
{
    private readonly IWordMeaningRepository _repo;
    private readonly WordBusinessRules      _rules;

    public DeleteWordMeaningCommandHandler(IWordMeaningRepository repo, WordBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<DeleteWordMeaningResponse> Handle(DeleteWordMeaningCommand request, CancellationToken ct)
    {
        await _rules.WordMeaningShouldExist(request.Id, ct);

        WordMeaning meaning = await _repo.GetAsync(m => m.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        await _repo.DeleteAsync(meaning, ct);
        return new DeleteWordMeaningResponse { Id = request.Id, Deleted = true };
    }
}
