

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IDocumentRepository<TDocument> where TDocument : class
{
    Task<TDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDocument>> GetAllAvailableAsync();
    Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate);
    Task AddAsync(TDocument document, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, TDocument document, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<TDocument>> GetPagedAsync(
        IPagedFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TDocument, TFilter>> GetPagedAsync<TFilter>(
        TFilter filter,
        Expression<Func<TDocument, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TFilter : class, IPagedFilter;

    Task<(IReadOnlyList<TDocument> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TDocument, bool>>? predicate,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
}
