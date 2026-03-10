using OasisWords.Application.Services.AiDialogueService;
using OasisWords.Core.Persistence.Repositories;
using OasisWords.Domain.Entities;
using OasisWords.Persistence.Contexts;

namespace OasisWords.Persistence.Repositories;

public class AiDialogueSessionRepository : EfRepositoryBase<AiDialogueSession, Guid, OasisWordsDbContext>, IAiDialogueSessionRepository
{
    public AiDialogueSessionRepository(OasisWordsDbContext context) : base(context) { }
}

public class AiDialogueMessageRepository : EfRepositoryBase<AiDialogueMessage, Guid, OasisWordsDbContext>, IAiDialogueMessageRepository
{
    public AiDialogueMessageRepository(OasisWordsDbContext context) : base(context) { }
}

public class AiDialogueTargetWordRepository : EfRepositoryBase<AiDialogueTargetWord, Guid, OasisWordsDbContext>, IAiDialogueTargetWordRepository
{
    public AiDialogueTargetWordRepository(OasisWordsDbContext context) : base(context) { }
}
