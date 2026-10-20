using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IWatchItemRepository : IDocumentRepository<WatchItemDocument>
{
    Task<WatchItemDocument?> GetByUserAndTmdbAsync(Guid userId, int tmdbId, MediaType type, CancellationToken ct = default);
    Task<IReadOnlyList<WatchItemDocument>> GetUserWatchListAsync(Guid userId, WatchStatus? status = null, MediaType? type = null, CancellationToken ct = default);
}
