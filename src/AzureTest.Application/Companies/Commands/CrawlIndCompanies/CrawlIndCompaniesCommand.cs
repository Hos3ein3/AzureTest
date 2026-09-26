using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Companies.Commands.CrawlIndCompanies;

public sealed record CrawlIndCompaniesCommand() : ICommand<CrawlIndCompaniesResult>;

public sealed record CrawlIndCompaniesResult(int NewlyAdded, int TotalProcessed);

public sealed class CrawlIndCompaniesCommandHandler : BaseCommandHandler<CrawlIndCompaniesCommand, CrawlIndCompaniesResult>
{
    private readonly IIndCrawlerService _indCrawlerService;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly AzureTest.Application.Interfaces.Services.ICompanyCacheService _cacheService;

    public CrawlIndCompaniesCommandHandler(
        IUnitOfWork unitOfWork,
        IIndCrawlerService indCrawlerService,
        INoSqlUnitOfWork noSqlUnitOfWork,
        AzureTest.Application.Interfaces.Services.ICompanyCacheService cacheService,
        ILogger<CrawlIndCompaniesCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _indCrawlerService = indCrawlerService;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cacheService = cacheService;
    }

    protected override async Task<CrawlIndCompaniesResult> ExecuteAsync(CrawlIndCompaniesCommand command, CancellationToken cancellationToken)
    {
        string? excelFilePath = null;
        try
        {
            excelFilePath = await _indCrawlerService.CrawlAndSaveToExcelAsync(cancellationToken);
            var parsedCompanies = await _indCrawlerService.ReadCompaniesFromExcelAsync(excelFilePath, cancellationToken);

            var existingCompanies = await _noSqlUnitOfWork.Companies.GetAllAsync();
            var existingByKvk = existingCompanies
                .Where(c => !string.IsNullOrWhiteSpace(c.KvkNumber))
                .ToDictionary(c => c.KvkNumber!, c => c);

            int newAddedCount = 0;

            foreach (var parsed in parsedCompanies)
            {
                if (existingByKvk.TryGetValue(parsed.KvkNumber, out var existing))
                {
                    // Update if name or IND flag changed
                    if (!existing.IsINDMember || existing.CompanyName != parsed.CompanyName)
                    {
                        existing.IsINDMember = true;
                        existing.CompanyName = parsed.CompanyName;
                        await _noSqlUnitOfWork.Companies.UpdateAsync(existing.Id, existing);
                    }
                }
                else
                {
                    var newCompany = new Company
                    {
                        CompanyName = parsed.CompanyName,
                        KvkNumber = parsed.KvkNumber,
                        IsINDMember = true
                    };
                    await _noSqlUnitOfWork.Companies.AddAsync(newCompany);
                    newAddedCount++;
                }
            }

            if (newAddedCount > 0 || existingCompanies.Count != existingByKvk.Count)
            {
                // In a more precise implementation, we'd check if any existing companies were actually updated.
                // For simplicity, we just invalidate if anything was added.
            }
            // Always invalidate to be safe, since updates might have happened.
            await _cacheService.InvalidateCompaniesAsync(cancellationToken);

            return new CrawlIndCompaniesResult(newAddedCount, parsedCompanies.Count);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(excelFilePath) && File.Exists(excelFilePath))
            {
                try { File.Delete(excelFilePath); }
                catch { /* ignore cleanup errors */ }
            }
        }
    }
}
