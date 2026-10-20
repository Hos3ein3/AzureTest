using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Entities.Pomodoro;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSetting;

public sealed record UpdatePomodoroSettingCommand(Guid UserId, UpdatePomodoroSettingDto Dto) : ICommand<Result>;

public sealed class UpdatePomodoroSettingCommandValidator : AbstractValidator<UpdatePomodoroSettingCommand>
{
    public UpdatePomodoroSettingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();

        RuleFor(x => x.Dto.WorkDurationMinutes)
            .InclusiveBetween(PomodoroSetting.Rules.WorkDurationMinutesMin, PomodoroSetting.Rules.WorkDurationMinutesMax);

        RuleFor(x => x.Dto.ShortBreakMinutes)
            .InclusiveBetween(PomodoroSetting.Rules.ShortBreakMinutesMin, PomodoroSetting.Rules.ShortBreakMinutesMax);

        RuleFor(x => x.Dto.LongBreakMinutes)
            .InclusiveBetween(PomodoroSetting.Rules.LongBreakMinutesMin, PomodoroSetting.Rules.LongBreakMinutesMax);

        RuleFor(x => x.Dto.LongBreakInterval)
            .InclusiveBetween(PomodoroSetting.Rules.LongBreakIntervalMin, PomodoroSetting.Rules.LongBreakIntervalMax);
    }
}

public sealed class UpdatePomodoroSettingCommandHandler : ICommandHandler<UpdatePomodoroSettingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePomodoroSettingCommandHandler> _logger;

    public UpdatePomodoroSettingCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdatePomodoroSettingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdatePomodoroSettingCommand command, CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.Pomodoros.GetUserSettingAsync(command.UserId, cancellationToken);
        if (setting == null)
        {
            setting = PomodoroSetting.CreateDefault(command.UserId);
            setting.Update(
                workDurationMinutes: command.Dto.WorkDurationMinutes,
                shortBreakMinutes: command.Dto.ShortBreakMinutes,
                longBreakMinutes: command.Dto.LongBreakMinutes,
                longBreakInterval: command.Dto.LongBreakInterval,
                autoStartBreaks: command.Dto.AutoStartBreaks,
                autoStartPomodoros: command.Dto.AutoStartPomodoros,
                soundEnabled: command.Dto.SoundEnabled,
                defaultSkin: command.Dto.DefaultSkin);

            await _unitOfWork.Pomodoros.AddSettingAsync(setting, cancellationToken);
        }
        else
        {
            setting.Update(
                workDurationMinutes: command.Dto.WorkDurationMinutes,
                shortBreakMinutes: command.Dto.ShortBreakMinutes,
                longBreakMinutes: command.Dto.LongBreakMinutes,
                longBreakInterval: command.Dto.LongBreakInterval,
                autoStartBreaks: command.Dto.AutoStartBreaks,
                autoStartPomodoros: command.Dto.AutoStartPomodoros,
                soundEnabled: command.Dto.SoundEnabled,
                defaultSkin: command.Dto.DefaultSkin);

            await _unitOfWork.Pomodoros.UpdateSettingAsync(setting, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Pomodoro settings updated for user {UserId}", command.UserId);

        return Result.Success();
    }
}
