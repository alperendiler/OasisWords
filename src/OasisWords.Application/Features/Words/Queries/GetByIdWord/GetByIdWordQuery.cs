using MediatR;
using OasisWords.Application.Features.Words.Rules;
using OasisWords.Application.Services.WordService;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Domain.Entities;
using OasisWords.Domain.Enums;

namespace OasisWords.Application.Features.Words.Queries.GetByIdWord;

public class GetByIdWordQuery : IRequest<GetByIdWordResponse>
{
    public Guid Id { get; set; }
}

public class GetByIdWordResponse
{
    public Guid                         Id                   { get; set; }
    public string                       Text                 { get; set; } = string.Empty;
    public string?                      PhoneticSpelling     { get; set; }
    public string?                      PronunciationAudioUrl { get; set; }
    public string                       LanguageName         { get; set; } = string.Empty;
    public string                       LanguageCode         { get; set; } = string.Empty;
    public IList<WordMeaningDetailDto>  Meanings             { get; set; } = new List<WordMeaningDetailDto>();
}

public class WordMeaningDetailDto
{
    public Guid      Id                  { get; set; }
    public CefrLevel CefrLevel           { get; set; }
    public string    CefrLabel           { get; set; } = string.Empty;
    public string    TranslationLanguage { get; set; } = string.Empty;
    public string    TranslationText     { get; set; } = string.Empty;
    public string?   ExampleSentence     { get; set; }
    public string?   ExampleTranslation  { get; set; }
}

public class GetByIdWordQueryHandler : IRequestHandler<GetByIdWordQuery, GetByIdWordResponse>
{
    private readonly IWordRepository  _repo;
    private readonly WordBusinessRules _rules;

    public GetByIdWordQueryHandler(IWordRepository repo, WordBusinessRules rules)
    {
        _repo  = repo;
        _rules = rules;
    }

    public async Task<GetByIdWordResponse> Handle(GetByIdWordQuery request, CancellationToken ct)
    {
        await _rules.WordShouldExist(request.Id, ct);

        Word word = await _repo.GetAsync(
            predicate: w => w.Id == request.Id,
            include: q => q
                .Include(w => w.Language)
                .Include(w => w.Meanings)
                    .ThenInclude(m => m.TranslationLanguage),
            enableTracking: false,
            cancellationToken: ct)
            ?? throw new NotFoundException($"Word {request.Id} not found.");

        return new GetByIdWordResponse
        {
            Id                    = word.Id,
            Text                  = word.Text,
            PhoneticSpelling      = word.PhoneticSpelling,
            PronunciationAudioUrl = word.PronunciationAudioUrl,
            LanguageName          = word.Language.Name,
            LanguageCode          = word.Language.Code,
            Meanings = word.Meanings
                .OrderBy(m => m.CefrLevel)
                .ThenBy(m => m.TranslationLanguage.Name)
                .Select(m => new WordMeaningDetailDto
                {
                    Id                  = m.Id,
                    CefrLevel           = m.CefrLevel,
                    CefrLabel           = m.CefrLevel.ToString(),
                    TranslationLanguage = m.TranslationLanguage.Name,
                    TranslationText     = m.TranslationText,
                    ExampleSentence     = m.ExampleSentence,
                    ExampleTranslation  = m.ExampleTranslation
                }).ToList()
        };
    }
}
