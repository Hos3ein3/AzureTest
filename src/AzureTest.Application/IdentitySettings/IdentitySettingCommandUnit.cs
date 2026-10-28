using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;

namespace AzureTest.Application.IdentitySettings;

public sealed record UpdateIdentitySettingCommand(
    Guid Id,
    string RedirectAfterRegister,
    string RedirectAfterLogin,
    List<string> RolesForRegister,
    bool StayLoginAfterRegister,
    bool LogoutOtherDevicesOnLogin = false
) : ICommand<Result>;

public sealed class UpdateIdentitySettingCommandValidator : AbstractValidator<UpdateIdentitySettingCommand>
{
    public UpdateIdentitySettingCommandValidator()
    {
        RuleFor(x => x.RedirectAfterRegister).NotEmpty();
        RuleFor(x => x.RedirectAfterLogin).NotEmpty();
    }
}

public sealed class UpdateIdentitySettingCommandHandler : ICommandHandler<UpdateIdentitySettingCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "IdentitySetting_Cache";

    public UpdateIdentitySettingCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IDistributedCache cache)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cache = cache;
    }

    public async Task<Result> HandleAsync(UpdateIdentitySettingCommand request, CancellationToken cancellationToken = default)
    {
        var setting = await _noSqlUnitOfWork.IdentitySettings.GetByIdAsync(request.Id);
        if (setting == null)
            return Result.Failure("Setting not found.");

        setting.RedirectAfterRegister = request.RedirectAfterRegister;
        setting.RedirectAfterLogin = request.RedirectAfterLogin;
        setting.RolesForRegister = request.RolesForRegister ?? new List<string>();
        setting.StayLoginAfterRegister = request.StayLoginAfterRegister;
        setting.LogoutOtherDevicesOnLogin = request.LogoutOtherDevicesOnLogin;

        await _noSqlUnitOfWork.IdentitySettings.UpdateAsync(setting.Id, setting);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        // Update Redis cache immediately
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(setting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, cancellationToken);

        return Result.Success();
    }
}