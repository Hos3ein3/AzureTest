using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Notifications;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullNotificationHistoryRepository : NullDocumentRepository<NotificationHistoryDocument>, INotificationHistoryRepository
{
    public Task<IReadOnlyList<NotificationHistoryDocument>> GetByUserAsync(Guid userId, bool? unreadOnly = null, int limit = 50, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<NotificationHistoryDocument>>(Array.Empty<NotificationHistoryDocument>());

    public Task<long> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(0L);

    public Task<long> DeleteReadOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default) =>
        Task.FromResult(0L);
}

public class NullPushSubscriptionRepository : NullDocumentRepository<PushSubscriptionDocument>, IPushSubscriptionRepository
{
    public Task<IReadOnlyList<PushSubscriptionDocument>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<PushSubscriptionDocument>>(Array.Empty<PushSubscriptionDocument>());

    public Task<bool> DeleteByEndpointAsync(Guid userId, string endpoint, CancellationToken ct = default) =>
        Task.FromResult(true);
}
