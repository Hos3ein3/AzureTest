using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.UserPreferences.Commands;

public sealed record UpdateUserPreferenceCommand(Guid UserId, string Language, string ThemeColor) : ICommand;

public sealed class UpdateUserPreferenceCommandHandler : ICommandHandler<UpdateUserPreferenceCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public UpdateUserPreferenceCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task HandleAsync(UpdateUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        var pref = await _noSqlUnitOfWork.UserPreferences.GetByUserIdAsync(request.UserId, cancellationToken);
        bool isNew = false;
        if (pref == null)
        {
            pref = new UserPreference { UserId = request.UserId };
            isNew = true;
        }

        pref.Language = request.Language;
        pref.ThemeColor = request.ThemeColor;

        if (isNew)
        {
            await _noSqlUnitOfWork.UserPreferences.AddAsync(pref, cancellationToken);
        }
        else
        {
            await _noSqlUnitOfWork.UserPreferences.UpdateAsync(pref, cancellationToken);
        }
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        var cacheKey = $"UserPref_{request.UserId}";
        await _redisService.SetAsync(cacheKey, pref, TimeSpan.FromHours(1));
    }
}
