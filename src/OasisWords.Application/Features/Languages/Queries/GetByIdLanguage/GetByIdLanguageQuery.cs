using MediatR;
using OasisWords.Application.Features.Languages.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Languages.Queries.GetByIdLanguage;

public class GetByIdLanguageQuery : IRequest<GetByIdLanguageResponse>
{
    public Guid Id { get; set; }
}

public class GetByIdLanguageResponse
{
    public Guid    Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }
    public int     WordCount    { get; set; }
}

public class GetByIdLanguageQueryHandler : IRequestHandler<GetByIdLanguageQuery, GetByIdLanguageResponse>
{
    private readonly ILanguageRepository   _repo;
    private readonly LanguageBusinessRules _rules;

    public GetByIdLanguageQueryHandler(ILanguageRepository repo, LanguageBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<GetByIdLanguageResponse> Handle(GetByIdLanguageQuery request, CancellationToken ct)
    {
        await _rules.LanguageShouldExist(request.Id, ct);

        Language lang = await _repo.GetAsync(
            l => l.Id == request.Id,
            include: q => q.Include(l => l.Words),
            enableTracking: false,
            cancellationToken: ct)
            ?? throw new NotFoundException($"Language {request.Id} not found.");

        return new GetByIdLanguageResponse
        {
            Id           = lang.Id,
            Name         = lang.Name,
            Code         = lang.Code,
            FlagImageUrl = lang.FlagImageUrl,
            WordCount    = lang.Words.Count
        };
    }
}
