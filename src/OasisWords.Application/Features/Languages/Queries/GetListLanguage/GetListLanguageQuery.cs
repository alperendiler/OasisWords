using MediatR;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.Application.Requests;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Languages.Queries.GetListLanguage;

public class GetListLanguageQuery : IRequest<GetListLanguageResponse>
{
    public PageRequest PageRequest { get; set; } = new();
}

public class GetListLanguageResponse
{
    public IList<LanguageListItemDto> Items { get; set; } = new List<LanguageListItemDto>();
    public int Count { get; set; }
    public int Pages { get; set; }
}

public class LanguageListItemDto
{
    public Guid    Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? FlagImageUrl { get; set; }
    public int     WordCount    { get; set; }
}

public class GetListLanguageQueryHandler : IRequestHandler<GetListLanguageQuery, GetListLanguageResponse>
{
    private readonly ILanguageRepository _repo;

    public GetListLanguageQueryHandler(ILanguageRepository repo) => _repo = repo;

    public async Task<GetListLanguageResponse> Handle(GetListLanguageQuery request, CancellationToken ct)
    {
        IPaginate<Language> page = await _repo.GetListAsync(
            include: q => q.Include(l => l.Words),
            orderBy: q => q.OrderBy(l => l.Name),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: ct);

        return new GetListLanguageResponse
        {
            Items = page.Items.Select(l => new LanguageListItemDto
            {
                Id           = l.Id,
                Name         = l.Name,
                Code         = l.Code,
                FlagImageUrl = l.FlagImageUrl,
                WordCount    = l.Words.Count
            }).ToList(),
            Count = page.Count,
            Pages = page.Pages
        };
    }
}
