using OasisWords.Application.Services.WordService;
using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;
using OasisWords.Persistence.Contexts;

namespace OasisWords.Persistence.Repositories;

public class WordRepository : EfRepositoryBase<Word, Guid, OasisWordsDbContext>, IWordRepository
{
    public WordRepository(OasisWordsDbContext context) : base(context) { }
}

public class WordMeaningRepository : EfRepositoryBase<WordMeaning, Guid, OasisWordsDbContext>, IWordMeaningRepository
{
    public WordMeaningRepository(OasisWordsDbContext context) : base(context) { }
}

public class LanguageRepository : EfRepositoryBase<Language, Guid, OasisWordsDbContext>, ILanguageRepository
{
    public LanguageRepository(OasisWordsDbContext context) : base(context) { }
}
