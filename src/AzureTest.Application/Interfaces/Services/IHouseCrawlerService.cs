using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Interfaces.Services;

public interface IHouseCrawlerService
{
    Task<List<HouseCrawlResult>> CrawlAsync(string city, string minPrice, string maxPrice, string bedrooms, HouseWebsite website, HouseCrawlerCredential? credential, CancellationToken ct = default);
}

public class HouseCrawlResult
{
    public string Title { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool LoggedInSuccessfully { get; set; }
}
