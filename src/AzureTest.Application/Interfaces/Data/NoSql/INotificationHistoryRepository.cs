using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Notifications;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface INotificationHistoryRepository : IDocumentRepository<NotificationHistoryDocument>
{
    Task<IReadOnlyList<NotificationHistoryDocument>> GetByUserAsync(Guid userId, bool? unreadOnly = null, int limit = 50, CancellationToken ct = default);
    Task<long> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<long> DeleteReadOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default);
    Task<long> DeleteByUserAndDateRangeAsync(Guid userId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<bool> DeleteByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
}
