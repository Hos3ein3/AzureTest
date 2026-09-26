using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Tasks.Queries;
using AzureTest.Contracts.DTOs.Tasks;

namespace AzureTest.Application.Tasks;

public class TaskQueryUnit : ITaskQueryUnit
{
    public IQueryHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>> GetAllTasks { get; }
    public IQueryHandler<GetTaskByIdQuery, TaskDto?> GetTaskById { get; }

    public TaskQueryUnit(
        IQueryHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>> getAllTasks,
        IQueryHandler<GetTaskByIdQuery, TaskDto?> getTaskById)
    {
        GetAllTasks = getAllTasks;
        GetTaskById = getTaskById;
    }
}
