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

public record GetUserHouseSearchHistoryQuery(Guid UserId) : IQuery<IReadOnlyList<HouseSearchHistory>>;

public class GetUserHouseSearchHistoryQueryHandler : BaseQueryHandler<GetUserHouseSearchHistoryQuery, IReadOnlyList<HouseSearchHistory>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public GetUserHouseSearchHistoryQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, ILogger<GetUserHouseSearchHistoryQueryHandler> logger) : base(logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    protected override async Task<IReadOnlyList<HouseSearchHistory>> ExecuteAsync(GetUserHouseSearchHistoryQuery query, CancellationToken cancellationToken)
    {
        var all = await _noSqlUnitOfWork.HouseSearchHistories.GetAllAsync(cancellationToken);
        return all.Where(h => h.UserId == query.UserId).OrderByDescending(h => h.SearchDate).ToList();
    }
}
