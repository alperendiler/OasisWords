using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Services.WordService;

public interface IWordRepository : IAsyncRepository<Word, Guid>, IRepository<Word, Guid> { }

public interface IWordMeaningRepository : IAsyncRepository<WordMeaning, Guid>, IRepository<WordMeaning, Guid> { }

public interface ILanguageRepository : IAsyncRepository<Language, Guid>, IRepository<Language, Guid> { }
