using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Tasks.Queries;
using AzureTest.Contracts.DTOs.Tasks;

namespace AzureTest.Application.Tasks;

public interface ITaskQueryUnit
{
    IQueryHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>> GetAllTasks { get; }
    IQueryHandler<GetTaskByIdQuery, TaskDto?> GetTaskById { get; }
}
