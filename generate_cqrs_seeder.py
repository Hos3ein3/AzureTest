import os

def ensure_dir(path):
    os.makedirs(os.path.dirname(path), exist_ok=True)

def write_file(path, content):
    ensure_dir(path)
    with open(path, "w") as f:
        f.write(content.strip())

# 7. CQRS Query Unit
write_file("src/AzureTest.Application/IdentitySettings/IdentitySettingQueryUnit.cs", """
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Caching.Distributed;

namespace AzureTest.Application.IdentitySettings;

public sealed record GetIdentitySettingQuery() : IQuery<Result<IdentitySetting>>;

public sealed class GetIdentitySettingQueryHandler : BaseQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "IdentitySetting_Cache";

    public GetIdentitySettingQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, IDistributedCache cache)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cache = cache;
    }

    public override async Task<Result<IdentitySetting>> HandleAsync(GetIdentitySettingQuery request, CancellationToken cancellationToken = default)
    {
        // 1. Try Redis
        var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            var settingFromCache = JsonSerializer.Deserialize<IdentitySetting>(cached);
            if (settingFromCache != null)
                return Result<IdentitySetting>.Success(settingFromCache);
        }

        // 2. Fetch from Mongo
        var list = await _noSqlUnitOfWork.IdentitySettings.GetAllAsync();
        var setting = list.Count > 0 ? list[0] : null;

        if (setting == null)
            return Result<IdentitySetting>.Failure("Identity setting not found.");

        // 3. Save to Redis
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(setting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, cancellationToken);

        return Result<IdentitySetting>.Success(setting);
    }
}
""")

# 8. CQRS Command Unit
write_file("src/AzureTest.Application/IdentitySettings/IdentitySettingCommandUnit.cs", """
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
    bool StayLoginAfterRegister
) : ICommand<Result>;

public sealed class UpdateIdentitySettingCommandValidator : AbstractValidator<UpdateIdentitySettingCommand>
{
    public UpdateIdentitySettingCommandValidator()
    {
        RuleFor(x => x.RedirectAfterRegister).NotEmpty();
        RuleFor(x => x.RedirectAfterLogin).NotEmpty();
    }
}

public sealed class UpdateIdentitySettingCommandHandler : BaseCommandHandler<UpdateIdentitySettingCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "IdentitySetting_Cache";

    public UpdateIdentitySettingCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IDistributedCache cache)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cache = cache;
    }

    public override async Task<Result> HandleAsync(UpdateIdentitySettingCommand request, CancellationToken cancellationToken = default)
    {
        var setting = await _noSqlUnitOfWork.IdentitySettings.GetByIdAsync(request.Id);
        if (setting == null)
            return Result.Failure("Setting not found.");

        setting.RedirectAfterRegister = request.RedirectAfterRegister;
        setting.RedirectAfterLogin = request.RedirectAfterLogin;
        setting.RolesForRegister = request.RolesForRegister ?? new List<string>();
        setting.StayLoginAfterRegister = request.StayLoginAfterRegister;

        await _noSqlUnitOfWork.IdentitySettings.UpdateAsync(setting);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        // Update Redis cache immediately
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(setting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, cancellationToken);

        return Result.Success();
    }
}
""")

# 9. AppSettings Seeding Config
# We'll patch IdentityDataSeeder.cs to read from configuration and seed IdentitySetting if it doesn't exist
seeder_path = "src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs"
with open(seeder_path, "r") as f:
    seeder_content = f.read()

if "SeedIdentitySettingAsync" not in seeder_content:
    method = """
    private async Task SeedIdentitySettingAsync()
    {
        var settings = await _noSqlUnitOfWork.IdentitySettings.GetAllAsync();
        if (settings.Count == 0)
        {
            var conf = _configuration.GetSection("AppIdentitySetting");
            var setting = new AzureTest.Domain.Documents.IdentitySetting
            {
                Id = Guid.CreateVersion7(),
                RedirectAfterRegister = conf["RedirectAfterRegister"] ?? "/",
                RedirectAfterLogin = conf["RedirectAfterLogin"] ?? "/",
                StayLoginAfterRegister = bool.TryParse(conf["StayLoginAfterRegister"], out var b) ? b : true,
                RolesForRegister = conf.GetSection("RolesForRegister").Get<List<string>>() ?? new List<string> { "User" }
            };
            
            await _noSqlUnitOfWork.IdentitySettings.AddAsync(setting);
            await _noSqlUnitOfWork.SaveChangesAsync();
            _logger.LogInformation("IdentitySetting document seeded from appsettings.");
        }
    }
"""
    seeder_content = seeder_content.replace(
        "await SeedDashboardComponentsAsync(adminUser.Id);",
        "await SeedDashboardComponentsAsync(adminUser.Id);\n            await SeedIdentitySettingAsync();"
    )
    seeder_content = seeder_content.replace(
        "private async Task SeedDashboardComponentsAsync",
        method + "\n    private async Task SeedDashboardComponentsAsync"
    )
    with open(seeder_path, "w") as f:
        f.write(seeder_content)

print("CQRS and Seeder patched")
