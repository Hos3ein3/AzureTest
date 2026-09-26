using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Queries;

public record GetAllTasksQuery(Guid? UserId) : IQuery<IReadOnlyList<TaskDto>>;


public class GetAllTasksQueryHandler : BaseQueryHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTasksQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllTasksQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<IReadOnlyList<TaskDto>> ExecuteAsync(GetAllTasksQuery query, CancellationToken cancellationToken)
    {
        var tasks = await _unitOfWork.Tasks.GetTasksForUserAsync(query.UserId);

        return tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate ?? DateTime.MaxValue).Select(t => new TaskDto(
            t.Id, t.Title, t.Description, t.DueDate, t.IsCompleted, t.Priority, t.UserId
        )).ToList();
    }
}
