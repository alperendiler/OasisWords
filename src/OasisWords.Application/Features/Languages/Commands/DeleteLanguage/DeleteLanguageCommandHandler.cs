using MediatR;
using OasisWords.Application.Features.Languages.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Languages.Commands.DeleteLanguage;

public class DeleteLanguageCommandHandler : IRequestHandler<DeleteLanguageCommand, DeleteLanguageResponse>
{
    private readonly ILanguageRepository   _repo;
    private readonly LanguageBusinessRules _rules;

    public DeleteLanguageCommandHandler(ILanguageRepository repo, LanguageBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<DeleteLanguageResponse> Handle(DeleteLanguageCommand request, CancellationToken ct)
    {
        await _rules.LanguageShouldExist(request.Id, ct);

        Language lang = await _repo.GetAsync(l => l.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        await _repo.DeleteAsync(lang, ct);
        return new DeleteLanguageResponse { Id = request.Id, Deleted = true };
    }
}
