using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Media;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IFollowedPersonRepository : IDocumentRepository<FollowedPersonDocument>
{
    Task<FollowedPersonDocument?> GetByUserAndPersonAsync(Guid userId, int tmdbPersonId, CancellationToken ct = default);
    Task<IReadOnlyList<FollowedPersonDocument>> GetUserFollowedPeopleAsync(Guid userId, CancellationToken ct = default);
}
