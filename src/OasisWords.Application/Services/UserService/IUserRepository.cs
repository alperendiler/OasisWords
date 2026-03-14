using OasisWords.Core.Persistence.Repositories;
using OasisWords.Core.Security.Entities;

namespace OasisWords.Application.Services.UserService;

public interface IUserRepository : IAsyncRepository<User, Guid>, IRepository<User, Guid> { }

public interface IRefreshTokenRepository : IAsyncRepository<RefreshToken, Guid>, IRepository<RefreshToken, Guid> { }

public interface IOperationClaimRepository : IAsyncRepository<OperationClaim, Guid>, IRepository<OperationClaim, Guid> { }

public interface IUserOperationClaimRepository : IAsyncRepository<UserOperationClaim, Guid>, IRepository<UserOperationClaim, Guid> { }
