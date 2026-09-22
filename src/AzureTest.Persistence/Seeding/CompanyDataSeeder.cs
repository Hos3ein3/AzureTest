using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Persistence.Seeding;

public class CompanyDataSeeder
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IIndCrawlerService _indCrawlerService;
    private readonly ILogger<CompanyDataSeeder> _logger;

    public CompanyDataSeeder(
        ICompanyRepository companyRepository,
        IIndCrawlerService indCrawlerService,
        ILogger<CompanyDataSeeder> logger)
    {
        _companyRepository = companyRepository;
        _indCrawlerService = indCrawlerService;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if Company document requires seeding...");

        var existingCompanies = await _companyRepository.GetAllAsync();
        if (existingCompanies.Any())
        {
            _logger.LogInformation("Company document already contains {Count} records. Skipping seed.", existingCompanies.Count);
            return;
        }

        _logger.LogInformation("Company document is empty. Starting IND crawler for initial seed.");
        
        string? excelFilePath = null;
        try
        {
            excelFilePath = await _indCrawlerService.CrawlAndSaveToExcelAsync(cancellationToken);
            var parsedCompanies = await _indCrawlerService.ReadCompaniesFromExcelAsync(excelFilePath, cancellationToken);
            
            _logger.LogInformation("Parsed {Count} companies from generated Excel. Inserting into database...", parsedCompanies.Count);

            foreach (var comp in parsedCompanies)
            {
                var document = new Company
                {
                    CompanyName = comp.CompanyName,
                    KvkNumber = comp.KvkNumber,
                    IsINDMember = true
                };
                await _companyRepository.AddAsync(document);
            }

            _logger.LogInformation("Successfully seeded {Count} companies.", parsedCompanies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed companies from IND.");
            throw;
        }
        finally
        {
            // Clean up the temporary excel file
            if (!string.IsNullOrWhiteSpace(excelFilePath) && File.Exists(excelFilePath))
            {
                try
                {
                    File.Delete(excelFilePath);
                    _logger.LogInformation("Deleted temporary Excel file at {Path}", excelFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary Excel file at {Path}", excelFilePath);
                }
            }
        }
    }
}
