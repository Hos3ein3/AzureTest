

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IUserDashboardRepository : IDocumentRepository<UserDashboard>
{
    Task<IEnumerable<UserDashboard>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDashboard?> GetByUserIdAndSlugAsync(Guid userId, string slug, CancellationToken cancellationToken = default);
}
