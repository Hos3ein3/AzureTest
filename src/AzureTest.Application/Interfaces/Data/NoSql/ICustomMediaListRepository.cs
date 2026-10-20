using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Media;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface ICustomMediaListRepository : IDocumentRepository<CustomMediaListDocument>
{
    Task<IReadOnlyList<CustomMediaListDocument>> GetUserListsAsync(Guid userId, CancellationToken ct = default);
}
