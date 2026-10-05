using System.Collections.Generic;
using AzureTest.Application.HouseCrawler.Interfaces;
using AzureTest.Application.HouseCrawler.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.HouseCrawler;

public class HouseCrawlerQueryUnit : IHouseCrawlerQueryUnit
{
    public IQueryHandler<GetUserHouseSearchHistoryQuery, IReadOnlyList<HouseSearchHistory>> GetUserHistory { get; }
    public IQueryHandler<GetUserHouseCredentialsQuery, IReadOnlyList<HouseCrawlerCredential>> GetUserCredentials { get; }

    public HouseCrawlerQueryUnit(
        IQueryHandler<GetUserHouseSearchHistoryQuery, IReadOnlyList<HouseSearchHistory>> getUserHistory,
        IQueryHandler<GetUserHouseCredentialsQuery, IReadOnlyList<HouseCrawlerCredential>> getUserCredentials)
    {
        GetUserHistory = getUserHistory;
        GetUserCredentials = getUserCredentials;
    }
}
