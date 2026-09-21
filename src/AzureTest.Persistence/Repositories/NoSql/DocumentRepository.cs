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

    public virtual async Task<TDocument?> GetByIdAsync(Guid id)
    {
        // Use the BSON _id field name explicitly; the class map ensures Guid is mapped to _id
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IReadOnlyList<TDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<IReadOnlyList<TDocument>> FindAsync(Expression<Func<TDocument, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(TDocument document)
    {
        await _collection.InsertOneAsync(document);
    }

    public virtual async Task UpdateAsync(Guid id, TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false });
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        var filter = Builders<TDocument>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter);
    }
}
