using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

/// <summary>
/// Generic repository that can operate against any DbContext (Master or ReadOnly replica).
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? predicate,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var prop = typeof(TEntity).GetProperties().FirstOrDefault(p => string.Equals(p.Name, sortBy, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var propertyAccess = Expression.Property(parameter, prop);
                var lambda = Expression.Lambda(propertyAccess, parameter);
                string methodName = sortDescending ? "OrderByDescending" : "OrderBy";
                var resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(TEntity), prop.PropertyType }, query.Expression, Expression.Quote(lambda));
                query = query.Provider.CreateQuery<TEntity>(resultExp);
            }
            else
            {
                query = sortDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id);
            }
        }
        else
        {
            query = query.OrderByDescending(e => e.Id);
        }

        int page = pageNumber < 1 ? 1 : pageNumber;
        int size = pageSize < 1 ? 10 : pageSize;

        var items = pageSize <= 0
            ? await query.ToListAsync(cancellationToken)
            : await query.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        IPagedFilter filter,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = ApplyPagedFilter(filter, predicate);

        int totalCount = await query.CountAsync(cancellationToken);
        int page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        int size = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = filter.PageSize <= 0
            ? await query.ToListAsync(cancellationToken)
            : await query.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, totalCount, page, size, filter);
    }

    public virtual async Task<PagedResult<TEntity, TFilter>> GetPagedAsync<TFilter>(
        TFilter filter,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TFilter : class, IPagedFilter
    {
        IQueryable<TEntity> query = ApplyPagedFilter(filter, predicate);

        int totalCount = await query.CountAsync(cancellationToken);
        int page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        int size = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = filter.PageSize <= 0
            ? await query.ToListAsync(cancellationToken)
            : await query.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);

        return new PagedResult<TEntity, TFilter>(items, totalCount, page, size, filter);
    }

    protected virtual IQueryable<TEntity> ApplyPagedFilter(IPagedFilter filter, Expression<Func<TEntity, bool>>? predicate)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // Apply UserIds filter if entity has UserId property
        if (filter.UserIds != null && filter.UserIds.Any())
        {
            var userIdProp = typeof(TEntity).GetProperty("UserId");
            if (userIdProp != null && userIdProp.PropertyType == typeof(Guid))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var propertyAccess = Expression.Property(parameter, userIdProp);
                var userIdsConst = Expression.Constant(filter.UserIds);
                var containsMethod = typeof(List<Guid>).GetMethod("Contains", new[] { typeof(Guid) })!;
                var body = Expression.Call(userIdsConst, containsMethod, propertyAccess);
                query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
            }
        }

        // Apply CreatedOn date filters if entity has CreatedOn property
        if (filter.CreatedOnFrom.HasValue || filter.CreatedOnTo.HasValue)
        {
            var createdOnProp = typeof(TEntity).GetProperty("CreatedOn");
            if (createdOnProp != null && (createdOnProp.PropertyType == typeof(DateTime) || createdOnProp.PropertyType == typeof(DateTime?)))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var propertyAccess = Expression.Property(parameter, createdOnProp);

                if (filter.CreatedOnFrom.HasValue)
                {
                    var fromConst = Expression.Constant(filter.CreatedOnFrom.Value, createdOnProp.PropertyType);
                    var greaterEqual = Expression.GreaterThanOrEqual(propertyAccess, fromConst);
                    query = query.Where(Expression.Lambda<Func<TEntity, bool>>(greaterEqual, parameter));
                }

                if (filter.CreatedOnTo.HasValue)
                {
                    var toDate = filter.CreatedOnTo.Value.TimeOfDay == TimeSpan.Zero
                        ? filter.CreatedOnTo.Value.Date.AddDays(1).AddTicks(-1)
                        : filter.CreatedOnTo.Value;
                    var toConst = Expression.Constant(toDate, createdOnProp.PropertyType);
                    var lessEqual = Expression.LessThanOrEqual(propertyAccess, toConst);
                    query = query.Where(Expression.Lambda<Func<TEntity, bool>>(lessEqual, parameter));
                }
            }
        }

        // Apply Sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var prop = typeof(TEntity).GetProperties().FirstOrDefault(p => string.Equals(p.Name, filter.SortBy, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var propertyAccess = Expression.Property(parameter, prop);
                var lambda = Expression.Lambda(propertyAccess, parameter);
                string methodName = filter.SortDescending ? "OrderByDescending" : "OrderBy";
                var resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(TEntity), prop.PropertyType }, query.Expression, Expression.Quote(lambda));
                query = query.Provider.CreateQuery<TEntity>(resultExp);
            }
            else
            {
                query = filter.SortDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id);
            }
        }
        else
        {
            query = query.OrderByDescending(e => e.Id);
        }

        return query;
    }
}
