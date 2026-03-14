using MediatR;
using OasisWords.Application.Features.Languages.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Languages.Commands.CreateLanguage;

public class CreateLanguageCommandHandler : IRequestHandler<CreateLanguageCommand, CreateLanguageResponse>
{
    private readonly ILanguageRepository    _languageRepository;
    private readonly LanguageBusinessRules  _rules;

    public CreateLanguageCommandHandler(ILanguageRepository languageRepository, LanguageBusinessRules rules)
    {
        _languageRepository = languageRepository;
        _rules = rules;
    }

    public async Task<CreateLanguageResponse> Handle(CreateLanguageCommand request, CancellationToken ct)
    {
        await _rules.LanguageCodeCannotBeDuplicated(request.Code, ct);

        Language lang = new()
        {
            Name         = request.Name,
            Code         = request.Code.ToLower().Trim(),
            FlagImageUrl = request.FlagImageUrl
        };

        Language created = await _languageRepository.AddAsync(lang, ct);
        return new CreateLanguageResponse { Id = created.Id, Name = created.Name, Code = created.Code };
    }
}
