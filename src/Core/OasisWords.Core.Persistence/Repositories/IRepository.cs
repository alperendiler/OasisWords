using OasisWords.Core.Persistence.Dynamic;
using OasisWords.Core.Persistence.Paging;
using System.Linq.Expressions;

namespace OasisWords.Core.Persistence.Repositories;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    TEntity? Get(Expression<Func<TEntity, bool>> predicate);
    IPaginate<TEntity> GetList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0, int size = 10, bool enableTracking = true);
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    TEntity Delete(TEntity entity);
}

public interface IAsyncRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<IPaginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0, int size = 10, bool enableTracking = true,
        CancellationToken cancellationToken = default);

    Task<IPaginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        int index = 0, int size = 10, bool enableTracking = true,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
