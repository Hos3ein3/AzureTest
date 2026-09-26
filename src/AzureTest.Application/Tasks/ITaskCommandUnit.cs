using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Tasks.Commands;

namespace AzureTest.Application.Tasks;

public interface ITaskCommandUnit
{
    ICommandHandler<CreateTaskCommand, Guid> CreateTask { get; }
    ICommandHandler<UpdateTaskCommand, Result> UpdateTask { get; }
    ICommandHandler<ToggleTaskCommand, Result> ToggleTask { get; }
    ICommandHandler<DeleteTaskCommand, Result> DeleteTask { get; }
}
