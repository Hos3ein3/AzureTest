using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Queries.GetSessionUser;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Auth;

public interface IAuthQueryUnit
{
    IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> GetIdentitySetting { get; }
    IQueryHandler<GetSessionUserQuery, Result<UserSessionCacheDto?>> GetSessionUser { get; }
}
