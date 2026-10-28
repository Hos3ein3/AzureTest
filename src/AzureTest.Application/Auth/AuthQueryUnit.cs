using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Queries.GetSessionUser;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Auth;

public class AuthQueryUnit : IAuthQueryUnit
{
    public IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> GetIdentitySetting { get; }
    public IQueryHandler<GetSessionUserQuery, Result<UserSessionCacheDto?>> GetSessionUser { get; }

    public AuthQueryUnit(
        IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> getIdentitySetting,
        IQueryHandler<GetSessionUserQuery, Result<UserSessionCacheDto?>> getSessionUser)
    {
        GetIdentitySetting = getIdentitySetting ?? throw new ArgumentNullException(nameof(getIdentitySetting));
        GetSessionUser = getSessionUser ?? throw new ArgumentNullException(nameof(getSessionUser));
    }
}
