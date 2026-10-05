using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.HouseCrawler.Queries;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.HouseCrawler.Interfaces;

public interface IHouseCrawlerQueryUnit
{
    IQueryHandler<GetUserHouseSearchHistoryQuery, IReadOnlyList<HouseSearchHistory>> GetUserHistory { get; }
    IQueryHandler<GetUserHouseCredentialsQuery, IReadOnlyList<HouseCrawlerCredential>> GetUserCredentials { get; }
}
