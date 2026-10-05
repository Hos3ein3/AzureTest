using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.HouseCrawler.Queries;

public record GetUserHouseCredentialsQuery(Guid UserId) : IQuery<IReadOnlyList<HouseCrawlerCredential>>;

public class GetUserHouseCredentialsQueryHandler : BaseQueryHandler<GetUserHouseCredentialsQuery, IReadOnlyList<HouseCrawlerCredential>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetUserHouseCredentialsQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, ILogger<GetUserHouseCredentialsQueryHandler> logger) : base(logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    protected override async Task<IReadOnlyList<HouseCrawlerCredential>> ExecuteAsync(GetUserHouseCredentialsQuery query, CancellationToken cancellationToken)
    {
        var all = await _noSqlUnitOfWork.HouseCrawlerCredentials.GetAllAsync(cancellationToken);
        return all.Where(c => c.UserId == query.UserId).ToList();
    }
}
