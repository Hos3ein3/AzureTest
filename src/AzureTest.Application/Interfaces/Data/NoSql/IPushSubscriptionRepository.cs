using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Notifications;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IPushSubscriptionRepository : IDocumentRepository<PushSubscriptionDocument>
{
    Task<IReadOnlyList<PushSubscriptionDocument>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> DeleteByEndpointAsync(Guid userId, string endpoint, CancellationToken ct = default);
}
