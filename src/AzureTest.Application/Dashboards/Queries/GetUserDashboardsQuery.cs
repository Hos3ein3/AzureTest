using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Dashboards.Queries;

public sealed record GetUserDashboardsQuery(Guid UserId) : IQuery<IEnumerable<UserDashboard>>;

public sealed class GetUserDashboardsQueryHandler : IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetUserDashboardsQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<IEnumerable<UserDashboard>> HandleAsync(GetUserDashboardsQuery request, CancellationToken cancellationToken)
    {
        return await _noSqlUnitOfWork.UserDashboards.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}
