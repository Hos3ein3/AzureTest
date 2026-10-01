

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using ClosedXML.Excel;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class IndCrawlerService : IIndCrawlerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IndCrawlerService> _logger;
    private const string IND_URL = "https://ind.nl/nl/openbaar-register-erkende-referenten/openbaar-register-arbeid";

    public IndCrawlerService(HttpClient httpClient, ILogger<IndCrawlerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> CrawlAndSaveToExcelAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting crawl of IND public register: {Url}", IND_URL);
        
        var response = await _httpClient.GetAsync(IND_URL, cancellationToken);
        response.EnsureSuccessStatusCode();
        var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Find the table within the .ind-table-wrapper
        var rows = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ind-table-wrapper')]//table/tbody/tr");
        if (rows == null || rows.Count == 0)
        {
            _logger.LogWarning("No data rows found in the IND table.");
            throw new Exception("Could not find the IND companies table in the fetched HTML.");
        }

        var companies = new List<(string Name, string Kvk)>();
        foreach (var row in rows)
        {
            var cols = row.SelectNodes("th|td");
            if (cols != null && cols.Count >= 2)
            {
                var name = System.Net.WebUtility.HtmlDecode(cols[0].InnerText.Trim());
                var kvk = System.Net.WebUtility.HtmlDecode(cols[1].InnerText.Trim());
                
                if (!string.IsNullOrWhiteSpace(name))
                {
                    companies.Add((name, kvk));
                }
            }
        }

        _logger.LogInformation("Successfully parsed {Count} companies from IND.", companies.Count);

        string filePath = Path.Combine(Path.GetTempPath(), $"IND_Companies_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Companies");
            worksheet.Cell(1, 1).Value = "Company Name";
            worksheet.Cell(1, 2).Value = "KVK Number";
            
            // Format header
            worksheet.Range("A1:B1").Style.Font.Bold = true;

            for (int i = 0; i < companies.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = companies[i].Name;
                
                // Keep KVK as text to preserve leading zeros
                worksheet.Cell(i + 2, 2).Value = $"'{companies[i].Kvk}"; 
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        _logger.LogInformation("Saved IND companies to Excel at {Path}", filePath);
        return filePath;
    }

    public async Task<IReadOnlyList<(string CompanyName, string KvkNumber)>> ReadCompaniesFromExcelAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Excel file not found at path: {filePath}");
        }

        var companies = new List<(string Name, string Kvk)>();
        
        await Task.Run(() => 
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed(); // Skip empty rows

            bool isFirstRow = true;
            foreach (var row in rows)
            {
                if (isFirstRow)
                {
                    isFirstRow = false;
                    continue; // Skip header
                }

                var name = row.Cell(1).GetString().Trim();
                var kvk = row.Cell(2).GetString().Trim();
                
                // Remove the explicit ' quote from reading if ClosedXML kept it
                if (kvk.StartsWith("'")) kvk = kvk.Substring(1);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    companies.Add((name, kvk));
                }
            }
        }, cancellationToken);

        return companies.AsReadOnly();
    }
}
