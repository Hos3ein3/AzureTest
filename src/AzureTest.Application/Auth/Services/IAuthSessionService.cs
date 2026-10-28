using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Identity;

namespace AzureTest.Application.Auth.Services;

public interface IAuthSessionService
{
    Task CacheUserSessionAsync(ApplicationUser user, CancellationToken ct = default);
    Task InvalidateUserSessionAsync(Guid userId, CancellationToken ct = default);
}
