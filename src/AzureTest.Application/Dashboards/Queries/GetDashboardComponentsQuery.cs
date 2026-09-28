using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Dashboards.Queries;

public sealed record GetDashboardComponentsQuery() : IQuery<IEnumerable<DashboardComponent>>;

public sealed class GetDashboardComponentsQueryHandler : IQueryHandler<GetDashboardComponentsQuery, IEnumerable<DashboardComponent>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetDashboardComponentsQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<IEnumerable<DashboardComponent>> HandleAsync(GetDashboardComponentsQuery request, CancellationToken cancellationToken)
    {
        return await _noSqlUnitOfWork.DashboardComponents.GetAllAsync(cancellationToken);
    }
}
