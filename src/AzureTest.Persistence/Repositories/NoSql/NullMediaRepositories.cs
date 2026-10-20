using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullWatchItemRepository : NullDocumentRepository<WatchItemDocument>, IWatchItemRepository
{
    public Task<WatchItemDocument?> GetByUserAndTmdbAsync(Guid userId, int tmdbId, MediaType type, CancellationToken ct = default) =>
        Task.FromResult<WatchItemDocument?>(null);

    public Task<IReadOnlyList<WatchItemDocument>> GetUserWatchListAsync(Guid userId, WatchStatus? status = null, MediaType? type = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<WatchItemDocument>>(Array.Empty<WatchItemDocument>());
}

public class NullFollowedPersonRepository : NullDocumentRepository<FollowedPersonDocument>, IFollowedPersonRepository
{
    public Task<FollowedPersonDocument?> GetByUserAndPersonAsync(Guid userId, int tmdbPersonId, CancellationToken ct = default) =>
        Task.FromResult<FollowedPersonDocument?>(null);

    public Task<IReadOnlyList<FollowedPersonDocument>> GetUserFollowedPeopleAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<FollowedPersonDocument>>(Array.Empty<FollowedPersonDocument>());
}

public class NullCustomMediaListRepository : NullDocumentRepository<CustomMediaListDocument>, ICustomMediaListRepository
{
    public Task<IReadOnlyList<CustomMediaListDocument>> GetUserListsAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<CustomMediaListDocument>>(Array.Empty<CustomMediaListDocument>());
}
