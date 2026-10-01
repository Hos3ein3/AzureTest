

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IDocumentRepository<TDocument> where TDocument : class
{
    Task<TDocument?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDocument>> GetAllAvailableAsync();
    Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate);
    Task AddAsync(TDocument document,CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, TDocument document,CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id,CancellationToken cancellationToken = default);
}
