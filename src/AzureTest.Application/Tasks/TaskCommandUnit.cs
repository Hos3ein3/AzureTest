using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;
using AzureTest.Application.Tasks.Commands;

namespace AzureTest.Application.Tasks;

public class TaskCommandUnit : ITaskCommandUnit
{
    public ICommandHandler<CreateTaskCommand, Result<Guid>> CreateTask { get; }
    public ICommandHandler<UpdateTaskCommand, Result> UpdateTask { get; }
    public ICommandHandler<ToggleTaskCommand, Result> ToggleTask { get; }
    public ICommandHandler<DeleteTaskCommand, Result> DeleteTask { get; }

    public TaskCommandUnit(
        ICommandHandler<CreateTaskCommand, Result<Guid>> createTask,
        ICommandHandler<UpdateTaskCommand, Result> updateTask,
        ICommandHandler<ToggleTaskCommand, Result> toggleTask,
        ICommandHandler<DeleteTaskCommand, Result> deleteTask)
    {
        CreateTask = createTask;
        UpdateTask = updateTask;
        ToggleTask = toggleTask;
        DeleteTask = deleteTask;
    }
}
