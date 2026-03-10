using AutoMapper;
using OasisWords.Application.Features.Words.DTOs;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Features.Words.Profiles;

public class WordMappingProfiles : Profile
{
    public WordMappingProfiles()
    {
        CreateMap<Word, WordListItemDto>()
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name))
            .ForMember(d => d.LanguageCode, o => o.MapFrom(s => s.Language.Code))
            .ForMember(d => d.MeaningCount, o => o.MapFrom(s => s.Meanings.Count));

        CreateMap<Word, WordDetailDto>()
            .ForMember(d => d.LanguageName, o => o.MapFrom(s => s.Language.Name));

        CreateMap<WordMeaning, WordMeaningDto>()
            .ForMember(d => d.TranslationLanguageName, o => o.MapFrom(s => s.TranslationLanguage.Name));
    }
}
