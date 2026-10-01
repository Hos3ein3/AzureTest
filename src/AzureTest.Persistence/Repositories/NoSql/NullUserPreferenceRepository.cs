

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NullUserPreferenceRepository : IUserPreferenceRepository
{
    public Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserPreference?>(null);
    }

    public Task AddAsync(UserPreference userPreference, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserPreference userPreference, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
