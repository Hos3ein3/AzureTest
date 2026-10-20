

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Primitives;

namespace AzureTest.Application.Interfaces.Data;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    Task<PagedResult<TEntity>> GetPagedAsync(
        IPagedFilter filter,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity, TFilter>> GetPagedAsync<TFilter>(
        TFilter filter,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TFilter : class, IPagedFilter;

    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>>? predicate,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
}
