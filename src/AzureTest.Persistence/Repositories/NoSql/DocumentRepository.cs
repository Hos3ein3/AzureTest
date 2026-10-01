

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;


public class DocumentRepository<TDocument> : IDocumentRepository<TDocument> where TDocument : class
{
    protected readonly IMongoCollection<TDocument> _collection;

    public DocumentRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<TDocument>(collectionName);
    }

    public virtual async Task<TDocument?> GetByIdAsync(Guid id,CancellationToken cancellationToken)
    {
        // Use the BSON _id field name explicitly; the class map ensures Guid is mapped to _id
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    

    public virtual async Task<IReadOnlyList<TDocument>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TDocument>> GetAllAvailableAsync()
    {
        if (typeof(AzureTest.Domain.Primitives.ISoftDelete).IsAssignableFrom(typeof(TDocument)))
        {
            var filter = Builders<TDocument>.Filter.Eq("IsDeleted", false);
            return await _collection.Find(filter).ToListAsync();
        }
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(TDocument document,CancellationToken cancellationToken )
    {
        await _collection.InsertOneAsync(document,cancellationToken);
    }

    public virtual async Task UpdateAsync(Guid id, TDocument document,CancellationToken cancellationToken )
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false },cancellationToken);
    }

    public virtual async Task RemoveAsync(Guid id,CancellationToken cancellationToken )
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter,cancellationToken);
    }
}
