using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;

using FluentValidation;

namespace AzureTest.Application.UserPreferences.Commands;

public sealed record UpdateUserPreferenceCommand(Guid UserId, string Language, string ThemeColor, string Currency, string PreferredDashboard, bool IsDarkMode, PreferredTimeZone? TimeZone = PreferredTimeZone.Utc) : ICommand;

public sealed class UpdateUserPreferenceCommandHandler : ICommandHandler<UpdateUserPreferenceCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public UpdateUserPreferenceCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<Result> HandleAsync(UpdateUserPreferenceCommand request, CancellationToken cancellationToken)
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
        pref.Currency = request.Currency;
        pref.PreferredDashboard = request.PreferredDashboard;
        pref.IsDarkMode = request.IsDarkMode;
        pref.TimeZone = request.TimeZone ?? pref.TimeZone;

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
        return Result.Success();
    }
}

public sealed class UpdateUserPreferenceCommandValidator : FluentValidation.AbstractValidator<UpdateUserPreferenceCommand>
{
    public UpdateUserPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Language).MaximumLength(10);
        RuleFor(x => x.ThemeColor).MaximumLength(50);
        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.PreferredDashboard).MaximumLength(100);
        RuleFor(x => x.TimeZone).IsInEnum();
    }
}
