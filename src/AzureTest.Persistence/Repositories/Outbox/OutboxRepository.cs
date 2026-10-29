using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.Outbox;
using AzureTest.Domain.Entities.Outbox;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.Outbox;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly AzureTestDbContext _context;

    public OutboxRepository(AzureTestDbContext context)
    {
        _context = context;
    }

    public Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 50, CancellationToken ct = default)
    {
        return _context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<(IReadOnlyList<OutboxMessage> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm = null,
        bool? isDone = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 200);

        var query = _context.OutboxMessages.AsNoTracking();

        // 1. Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(m => m.EventType.Contains(term) ||
                                     m.Payload.Contains(term) ||
                                     (m.Error != null && m.Error.Contains(term)));
        }

        // 2. IsDone filter
        if (isDone.HasValue)
        {
            query = isDone.Value
                ? query.Where(m => m.ProcessedOnUtc != null)
                : query.Where(m => m.ProcessedOnUtc == null);
        }

        // 3. Date Range (OccurredOnUtc)
        if (fromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(m => m.OccurredOnUtc >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(m => m.OccurredOnUtc <= toUtc);
        }

        // 4. Sorting
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "eventtype" => sortDescending ? query.OrderByDescending(m => m.EventType) : query.OrderBy(m => m.EventType),
            "processedon" or "processedonutc" or "done" => sortDescending ? query.OrderByDescending(m => m.ProcessedOnUtc) : query.OrderBy(m => m.ProcessedOnUtc),
            "retrycount" => sortDescending ? query.OrderByDescending(m => m.RetryCount) : query.OrderBy(m => m.RetryCount),
            _ => sortDescending ? query.OrderByDescending(m => m.OccurredOnUtc) : query.OrderBy(m => m.OccurredOnUtc)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }

    public void Update(OutboxMessage message)
    {
        _context.OutboxMessages.Update(message);
    }

    public async Task<int> DeleteByDateRangeAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var query = _context.OutboxMessages.AsQueryable();
        if (fromUtc.HasValue)
        {
            query = query.Where(m => m.OccurredOnUtc >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            query = query.Where(m => m.OccurredOnUtc <= toUtc.Value);
        }
        return await query.ExecuteDeleteAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await _context.OutboxMessages.Where(m => m.Id == id).ExecuteDeleteAsync(ct);
        return deleted > 0;
    }
}
