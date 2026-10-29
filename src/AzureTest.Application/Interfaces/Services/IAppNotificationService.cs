using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Interfaces.Services;

public interface IAppNotificationService
{
    Task<Result<Guid>> SendNotificationAsync(
        Guid userId,
        Guid tenantId,
        string title,
        string message,
        string type = "Trade",
        string? actionUrl = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken ct = default);
}
