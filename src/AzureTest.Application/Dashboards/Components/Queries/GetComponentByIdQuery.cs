using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Dashboards.Components.Queries;

public record GetComponentByIdQuery(Guid Id) : IQuery<DashboardComponent?>;

public class GetComponentByIdQueryHandler : IQueryHandler<GetComponentByIdQuery, DashboardComponent?>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetComponentByIdQueryHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardComponent?> HandleAsync(GetComponentByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.DashboardComponents.GetByIdAsync(query.Id, cancellationToken);
    }
}
