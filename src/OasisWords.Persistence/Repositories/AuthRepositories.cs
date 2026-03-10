using OasisWords.Application.Services.AuthService;
using OasisWords.Core.Persistence.Repositories;
using OasisWords.Core.Security.Entities;
using OasisWords.Persistence.Contexts;

namespace OasisWords.Persistence.Repositories;

public class UserRepository : EfRepositoryBase<User, Guid, OasisWordsDbContext>, IUserRepository
{
    public UserRepository(OasisWordsDbContext context) : base(context) { }
}

public class RefreshTokenRepository : EfRepositoryBase<RefreshToken, Guid, OasisWordsDbContext>, IRefreshTokenRepository
{
    public RefreshTokenRepository(OasisWordsDbContext context) : base(context) { }
}

public class OperationClaimRepository : EfRepositoryBase<OperationClaim, Guid, OasisWordsDbContext>, IOperationClaimRepository
{
    public OperationClaimRepository(OasisWordsDbContext context) : base(context) { }
}

public class UserOperationClaimRepository : EfRepositoryBase<UserOperationClaim, Guid, OasisWordsDbContext>, IUserOperationClaimRepository
{
    public UserOperationClaimRepository(OasisWordsDbContext context) : base(context) { }
}
