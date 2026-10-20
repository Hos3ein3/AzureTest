using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Pomodoro.Commands.AbandonPomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CompletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CreatePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.DeletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSetting;

namespace AzureTest.Application.Pomodoro;

public interface IPomodoroCommandUnit
{
    ICommandHandler<CreatePomodoroSessionCommand, Result<Guid>> CreateSession { get; }
    ICommandHandler<UpdatePomodoroSessionCommand, Result> UpdateSession { get; }
    ICommandHandler<CompletePomodoroSessionCommand, Result> CompleteSession { get; }
    ICommandHandler<AbandonPomodoroSessionCommand, Result> AbandonSession { get; }
    ICommandHandler<DeletePomodoroSessionCommand, Result> DeleteSession { get; }
    ICommandHandler<UpdatePomodoroSettingCommand, Result> UpdateSetting { get; }
}

public sealed class PomodoroCommandUnit : IPomodoroCommandUnit
{
    public ICommandHandler<CreatePomodoroSessionCommand, Result<Guid>> CreateSession { get; }
    public ICommandHandler<UpdatePomodoroSessionCommand, Result> UpdateSession { get; }
    public ICommandHandler<CompletePomodoroSessionCommand, Result> CompleteSession { get; }
    public ICommandHandler<AbandonPomodoroSessionCommand, Result> AbandonSession { get; }
    public ICommandHandler<DeletePomodoroSessionCommand, Result> DeleteSession { get; }
    public ICommandHandler<UpdatePomodoroSettingCommand, Result> UpdateSetting { get; }

    public PomodoroCommandUnit(
        ICommandHandler<CreatePomodoroSessionCommand, Result<Guid>> createSession,
        ICommandHandler<UpdatePomodoroSessionCommand, Result> updateSession,
        ICommandHandler<CompletePomodoroSessionCommand, Result> completeSession,
        ICommandHandler<AbandonPomodoroSessionCommand, Result> abandonSession,
        ICommandHandler<DeletePomodoroSessionCommand, Result> deleteSession,
        ICommandHandler<UpdatePomodoroSettingCommand, Result> updateSetting)
    {
        CreateSession = createSession;
        UpdateSession = updateSession;
        CompleteSession = completeSession;
        AbandonSession = abandonSession;
        DeleteSession = deleteSession;
        UpdateSetting = updateSetting;
    }
}
