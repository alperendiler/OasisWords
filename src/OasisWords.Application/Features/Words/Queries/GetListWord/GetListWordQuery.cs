using AutoMapper;
using MediatR;
using OasisWords.Application.Features.Words.DTOs;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.Application.Pipelines;
using OasisWords.Core.Application.Requests;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OasisWords.Application.Features.Words.Queries.GetListWord;

public class GetListWordQuery : IRequest<GetListWordResponse>, ICachableRequest
{
    public PageRequest PageRequest { get; set; } = new();
    public Guid? LanguageId { get; set; }

    public string CacheKey => $"GetListWord-{LanguageId}-{PageRequest.PageIndex}-{PageRequest.PageSize}";
    public bool BypassCache => false;
    public string? CacheGroupKey => "Words";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);
}

public class GetListWordResponse
{
    public IList<WordListItemDto> Items { get; set; } = new List<WordListItemDto>();
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class GetListWordQueryHandler : IRequestHandler<GetListWordQuery, GetListWordResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly IMapper _mapper;

    public GetListWordQueryHandler(IWordRepository wordRepository, IMapper mapper)
    {
        _wordRepository = wordRepository;
        _mapper = mapper;
    }

    public async Task<GetListWordResponse> Handle(GetListWordQuery request, CancellationToken cancellationToken)
    {
        IPaginate<Word> words = await _wordRepository.GetListAsync(
            predicate: request.LanguageId.HasValue
                ? w => w.LanguageId == request.LanguageId.Value
                : null,
            include: q => q
                .Include(w => w.Language)
                .Include(w => w.Meanings),
            orderBy: q => q.OrderBy(w => w.Text),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return new GetListWordResponse
        {
            Items = _mapper.Map<IList<WordListItemDto>>(words.Items),
            Index = words.Index,
            Size = words.Size,
            Count = words.Count,
            Pages = words.Pages,
            HasPrevious = words.HasPrevious,
            HasNext = words.HasNext
        };
    }
}
