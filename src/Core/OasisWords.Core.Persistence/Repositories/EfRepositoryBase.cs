using Microsoft.EntityFrameworkCore;
using OasisWords.Core.Persistence.Dynamic;
using OasisWords.Core.Persistence.Paging;
using System.Linq.Expressions;

namespace OasisWords.Core.Persistence.Repositories;

public class EfRepositoryBase<TEntity, TId, TContext>
    : IAsyncRepository<TEntity, TId>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TContext : DbContext
{
    protected readonly TContext Context;

    public EfRepositoryBase(TContext context)
    {
        Context = context;
    }

    // ── Sync ──────────────────────────────────────────────────────────────
    public TEntity? Get(Expression<Func<TEntity, bool>> predicate)
        => Context.Set<TEntity>().FirstOrDefault(predicate);

    public IPaginate<TEntity> GetList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0, int size = 10, bool enableTracking = true)
    {
        IQueryable<TEntity> query = enableTracking
            ? Context.Set<TEntity>()
            : Context.Set<TEntity>().AsNoTracking();

        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        return query.ToPaginate(index, size);
    }

    public TEntity Add(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        Context.Add(entity);
        Context.SaveChanges();
        return entity;
    }

    public TEntity Update(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Update(entity);
        Context.SaveChanges();
        return entity;
    }

    public TEntity Delete(TEntity entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        Context.Update(entity);
        Context.SaveChanges();
        return entity;
    }

    // ── Async ─────────────────────────────────────────────────────────────
    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Context.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IPaginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0, int size = 10, bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = enableTracking
            ? Context.Set<TEntity>()
            : Context.Set<TEntity>().AsNoTracking();

        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        int index = 0, int size = 10, bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>().AsQueryable().ToDynamic(dynamic);
        if (!enableTracking) query = query.AsNoTracking();
        if (predicate is not null) query = query.Where(predicate);
        return await query.ToPaginateAsync(index, size, cancellationToken: cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await Context.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.DeletedAt = DateTime.UtcNow;
        Context.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
}
