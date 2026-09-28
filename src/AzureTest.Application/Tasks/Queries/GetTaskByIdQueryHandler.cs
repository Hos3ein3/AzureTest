using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Queries;


public record GetTaskByIdQuery(Guid Id, Guid UserId) : IQuery<TaskDto?>;


public class GetTaskByIdQueryHandler : BaseQueryHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTaskByIdQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<TaskDto?> ExecuteAsync(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var t = await _unitOfWork.Tasks.GetByIdAsync(query.Id);
        if (t == null || t.UserId != query.UserId) return null;
        
        return new TaskDto(t.Id, t.Title, t.Description, t.StartDate, t.EndDate, t.IsCompleted, t.Priority, t.UserId);
    }
}
