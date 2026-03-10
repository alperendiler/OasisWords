using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;

namespace OasisWords.Application.Services.AiDialogueService;

public interface IAiDialogueSessionRepository : IAsyncRepository<AiDialogueSession, Guid>, IRepository<AiDialogueSession, Guid> { }

public interface IAiDialogueMessageRepository : IAsyncRepository<AiDialogueMessage, Guid>, IRepository<AiDialogueMessage, Guid> { }

public interface IAiDialogueTargetWordRepository : IAsyncRepository<AiDialogueTargetWord, Guid>, IRepository<AiDialogueTargetWord, Guid> { }
