using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Outbox;

namespace AzureTest.Application.Interfaces.Data.Outbox;

public interface IInboxRepository
{
    Task<bool> HasBeenProcessedAsync(Guid messageId, string handlerName, CancellationToken ct = default);
    Task<(IReadOnlyList<InboxMessage> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default);
    Task<InboxMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(InboxMessage message, CancellationToken ct = default);
    Task<int> DeleteByDateRangeAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
