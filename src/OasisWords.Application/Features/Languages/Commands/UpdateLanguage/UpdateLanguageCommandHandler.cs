using MediatR;
using OasisWords.Application.Features.Languages.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Languages.Commands.UpdateLanguage;

public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, UpdateLanguageResponse>
{
    private readonly ILanguageRepository   _repo;
    private readonly LanguageBusinessRules _rules;

    public UpdateLanguageCommandHandler(ILanguageRepository repo, LanguageBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<UpdateLanguageResponse> Handle(UpdateLanguageCommand request, CancellationToken ct)
    {
        await _rules.LanguageShouldExist(request.Id, ct);

        Language lang = await _repo.GetAsync(l => l.Id == request.Id, cancellationToken: ct)!
            ?? throw new InvalidOperationException();

        lang.Name         = request.Name;
        lang.FlagImageUrl = request.FlagImageUrl;

        Language updated = await _repo.UpdateAsync(lang, ct);
        return new UpdateLanguageResponse { Id = updated.Id, Name = updated.Name, Code = updated.Code };
    }
}
