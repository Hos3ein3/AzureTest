using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullDocumentRepository<TDocument> : IDocumentRepository<TDocument> where TDocument : class
{
    public virtual Task<TDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<TDocument?>(null);

    public virtual Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TDocument>>(Array.Empty<TDocument>());

    public virtual Task<IReadOnlyList<TDocument>> GetAllAvailableAsync() =>
        Task.FromResult<IReadOnlyList<TDocument>>(Array.Empty<TDocument>());

    public virtual Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<TDocument>>(Array.Empty<TDocument>());

    public virtual Task AddAsync(TDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual Task UpdateAsync(Guid id, TDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual Task<PagedResult<TDocument>> GetPagedAsync(
        IPagedFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<TDocument>(Array.Empty<TDocument>(), 0, filter.PageNumber, filter.PageSize, filter));

    public virtual Task<PagedResult<TDocument, TFilter>> GetPagedAsync<TFilter>(
        TFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TFilter : class, IPagedFilter =>
        Task.FromResult(new PagedResult<TDocument, TFilter>(Array.Empty<TDocument>(), 0, filter.PageNumber, filter.PageSize, filter));

    public virtual Task<(IReadOnlyList<TDocument> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TDocument, bool>>? predicate,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(((IReadOnlyList<TDocument>)Array.Empty<TDocument>(), 0));
}