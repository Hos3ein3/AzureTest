using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Dashboards.Queries;

public sealed record GetUserDashboardBySlugQuery(Guid UserId, string Slug) : IQuery<UserDashboard?>;

public sealed class GetUserDashboardBySlugQueryHandler : IQueryHandler<GetUserDashboardBySlugQuery, UserDashboard?>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetUserDashboardBySlugQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task<UserDashboard?> HandleAsync(GetUserDashboardBySlugQuery request, CancellationToken cancellationToken)
    {
        return await _noSqlUnitOfWork.UserDashboards.GetByUserIdAndSlugAsync(request.UserId, request.Slug, cancellationToken);
    }
}
