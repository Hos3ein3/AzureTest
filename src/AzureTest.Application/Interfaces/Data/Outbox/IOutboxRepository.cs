using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Outbox;

namespace AzureTest.Application.Interfaces.Data.Outbox;

public interface IOutboxRepository:IGenericRepository<OutboxMessage>
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 50, CancellationToken ct = default);
    Task<(IReadOnlyList<OutboxMessage> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm = null,
        bool? isDone = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default);
    
    
    Task<int> DeleteByDateRangeAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
