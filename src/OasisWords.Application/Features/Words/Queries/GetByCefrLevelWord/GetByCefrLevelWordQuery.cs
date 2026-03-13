using AutoMapper;
using MediatR;
using OasisWords.Application.Features.Words.DTOs;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.Application.Requests;
using OasisWords.Core.Persistence.Paging;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace OasisWords.Application.Features.Words.Queries.GetByCefrLevelWord;

public class GetByCefrLevelWordQuery : IRequest<GetByCefrLevelWordResponse>
{
    public CefrLevel CefrLevel { get; set; }
    public Guid LanguageId { get; set; }
    public Guid TranslationLanguageId { get; set; }
    public PageRequest PageRequest { get; set; } = new();
}

public class GetByCefrLevelWordResponse
{
    public IList<WordDetailDto> Items { get; set; } = new List<WordDetailDto>();
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class GetByCefrLevelWordQueryHandler : IRequestHandler<GetByCefrLevelWordQuery, GetByCefrLevelWordResponse>
{
    private readonly IWordRepository _wordRepository;
    private readonly IMapper _mapper;

    public GetByCefrLevelWordQueryHandler(IWordRepository wordRepository, IMapper mapper)
    {
        _wordRepository = wordRepository;
        _mapper = mapper;
    }

    public async Task<GetByCefrLevelWordResponse> Handle(
        GetByCefrLevelWordQuery request,
        CancellationToken cancellationToken)
    {
        IPaginate<Word> words = await _wordRepository.GetListAsync(
            predicate: w =>
                w.LanguageId == request.LanguageId &&
                w.Meanings.Any(m =>
                    m.CefrLevel == request.CefrLevel &&
                    m.TranslationLanguageId == request.TranslationLanguageId),
            include: q => q
                .Include(w => w.Language)
                .Include(w => w.Meanings.Where(m =>
                    m.CefrLevel == request.CefrLevel &&
                    m.TranslationLanguageId == request.TranslationLanguageId))
                    .ThenInclude(m => m.TranslationLanguage),
            orderBy: q => q.OrderBy(w => w.Text),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return new GetByCefrLevelWordResponse
        {
            Items = _mapper.Map<IList<WordDetailDto>>(words.Items),
            Index = words.Index,
            Size = words.Size,
            Count = words.Count,
            Pages = words.Pages,
            HasPrevious = words.HasPrevious,
            HasNext = words.HasNext
        };
    }
}
