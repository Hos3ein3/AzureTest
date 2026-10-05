using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class HouseCrawlerService : IHouseCrawlerService
{
    private readonly ILogger<HouseCrawlerService> _logger;

    public HouseCrawlerService(ILogger<HouseCrawlerService> logger)
    {
        _logger = logger;
    }

    public async Task<List<HouseCrawlResult>> CrawlAsync(string city, string minPrice, string maxPrice, string bedrooms, HouseWebsite website, HouseCrawlerCredential? credential, CancellationToken ct = default)
    {
        var results = new List<HouseCrawlResult>();
        
        bool isLoggedIn = false;
        
        // Simulate login process if credentials are provided
        if (credential != null && !string.IsNullOrWhiteSpace(credential.Username) && !string.IsNullOrWhiteSpace(credential.PasswordHash))
        {
            _logger.LogInformation($"Attempting login to {website} using credentials for {credential.Username}");
            // Simulate network delay for login
            await Task.Delay(500, ct); 
            isLoggedIn = true;
        }

        // Simulate scraping delay
        await Task.Delay(1000, ct);

        // Generate dummy results based on the source
        string sourceName = website.ToString();
        string baseUrl = website switch
        {
            HouseWebsite.Funda => "https://www.funda.nl",
            HouseWebsite.Pararius => "https://www.pararius.com",
            HouseWebsite.Huurportaal => "https://huurportaal.nl",
            HouseWebsite.Huurwoningen => "https://www.huurwoningen.com",
            HouseWebsite.Kamernet => "https://kamernet.nl",
            _ => "https://example.com"
        };

        for (int i = 1; i <= 3; i++)
        {
            results.Add(new HouseCrawlResult
            {
                Title = $"Beautiful Apartment {i} in {city}",
                Price = $"€{1000 + (i * 250)}",
                Location = $"{city} Center",
                Url = $"{baseUrl}/property/{Guid.NewGuid().ToString().Substring(0, 8)}",
                Source = sourceName,
                LoggedInSuccessfully = isLoggedIn
            });
        }

        return results;
    }
}
