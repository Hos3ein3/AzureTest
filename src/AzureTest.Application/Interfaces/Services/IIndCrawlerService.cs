using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface IIndCrawlerService
{
    /// <summary>
    /// Fetches the latest public register of recognized sponsors (Arbeid)
    /// from the IND website and extracts the companies and their KVK numbers.
    /// Saves the result as an Excel file and returns the file path.
    /// </summary>
    Task<string> CrawlAndSaveToExcelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the generated Excel file and returns the list of companies.
    /// </summary>
    Task<System.Collections.Generic.IReadOnlyList<(string CompanyName, string KvkNumber)>> ReadCompaniesFromExcelAsync(string filePath, CancellationToken cancellationToken = default);
}
