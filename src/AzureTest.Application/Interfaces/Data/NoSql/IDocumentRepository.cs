using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IDocumentRepository<TDocument> where TDocument : class
{
    Task<TDocument?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<TDocument>> GetAllAsync();
    Task<IReadOnlyList<TDocument>> GetAllAvailableAsync();
    Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate);
    Task AddAsync(TDocument document);
    Task UpdateAsync(Guid id, TDocument document);
    Task RemoveAsync(Guid id);
}
