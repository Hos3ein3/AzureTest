using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Entities.Pomodoro;

namespace AzureTest.Application.Pomodoro.Queries.GetPomodoroSetting;

public sealed record GetPomodoroSettingQuery(Guid UserId) : IQuery<Result<PomodoroSettingDto>>
{
    public static class Mapping
    {
        public static PomodoroSettingDto ToDto(PomodoroSetting s) =>
            new(
                s.Id,
                s.UserId,
                s.WorkDurationMinutes,
                s.ShortBreakMinutes,
                s.LongBreakMinutes,
                s.LongBreakInterval,
                s.AutoStartBreaks,
                s.AutoStartPomodoros,
                s.SoundEnabled,
                s.DefaultSkin);
    }
}

public sealed class GetPomodoroSettingQueryHandler : IQueryHandler<GetPomodoroSettingQuery, Result<PomodoroSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPomodoroSettingQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PomodoroSettingDto>> HandleAsync(GetPomodoroSettingQuery query, CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.Pomodoros.GetUserSettingAsync(query.UserId, cancellationToken);
        if (setting == null)
        {
            setting = PomodoroSetting.CreateDefault(query.UserId);
        }

        return Result<PomodoroSettingDto>.Success(GetPomodoroSettingQuery.Mapping.ToDto(setting));
    }
}
