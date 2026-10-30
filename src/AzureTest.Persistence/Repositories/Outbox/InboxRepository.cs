using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.Outbox;
using AzureTest.Domain.Entities.Outbox;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.Outbox;

public sealed class InboxRepository : IInboxRepository
{
    private readonly AzureTestDbContext _context;

    public InboxRepository(AzureTestDbContext context)
    {
        _context = context;
    }

    public Task<InboxMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.InboxMessages.FirstOrDefaultAsync(m => m.EventId == id, ct);
    }

    public async Task<(IReadOnlyList<InboxMessage> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm = null,
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

        var query = _context.InboxMessages.AsNoTracking();

        // 1. Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(m => m.HandlerName.Contains(term));
        }

        // 2. Date Range (ReceivedOnUtc)
        if (fromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(m => m.ReceivedOnUtc >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(m => m.ReceivedOnUtc <= toUtc);
        }

        // 3. Sorting
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "handlername" => sortDescending ? query.OrderByDescending(m => m.HandlerName) : query.OrderBy(m => m.HandlerName),
            "processedon" or "processedonutc" => sortDescending ? query.OrderByDescending(m => m.ProcessedOnUtc) : query.OrderBy(m => m.ProcessedOnUtc),
            _ => sortDescending ? query.OrderByDescending(m => m.ReceivedOnUtc) : query.OrderBy(m => m.ReceivedOnUtc)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<bool> HasBeenProcessedAsync(Guid messageId, string handlerName, CancellationToken ct = default)
    {
        return _context.InboxMessages
            .AnyAsync(m => m.EventId == messageId && m.HandlerName == handlerName, ct);
    }

    public async Task AddAsync(InboxMessage message, CancellationToken ct = default)
    {
        await _context.InboxMessages.AddAsync(message, ct);
    }

    public async Task<int> DeleteByDateRangeAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var query = _context.InboxMessages.AsQueryable();
        if (fromUtc.HasValue)
        {
            query = query.Where(m => m.ReceivedOnUtc >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            query = query.Where(m => m.ReceivedOnUtc <= toUtc.Value);
        }
        return await query.ExecuteDeleteAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await _context.InboxMessages.Where(m => m.EventId == id).ExecuteDeleteAsync(ct);
        return deleted > 0;
    }
}
