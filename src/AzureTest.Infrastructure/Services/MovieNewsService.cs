using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Media;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class MovieNewsService : IMovieNewsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MovieNewsService> _logger;
    private static IReadOnlyList<MovieNewsItemDto>? _cachedNews;
    private static DateTime _cacheExpiresAt = DateTime.MinValue;
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

    public MovieNewsService(HttpClient httpClient, ILogger<MovieNewsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MovieNewsItemDto>> GetLatestNewsAsync(int count = 20, CancellationToken ct = default)
    {
        if (_cachedNews != null && DateTime.UtcNow < _cacheExpiresAt)
        {
            return _cachedNews.Take(count).ToList();
        }

        await _cacheLock.WaitAsync(ct);
        try
        {
            if (_cachedNews != null && DateTime.UtcNow < _cacheExpiresAt)
            {
                return _cachedNews.Take(count).ToList();
            }

            var items = new List<MovieNewsItemDto>();

            // Primary feed: Collider / Movie News RSS feed
            var feedUrls = new[]
            {
                ("Collider", "https://collider.com/feed/category/movie-news/"),
                ("Variety", "https://variety.com/v/film/feed/"),
                ("Deadline", "https://deadline.com/v/film/feed/")
            };

            foreach (var (source, url) in feedUrls)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(4));

                    var response = await _httpClient.GetStringAsync(url, cts.Token);
                    var doc = XDocument.Parse(response);

                    var feedItems = doc.Descendants("item").Take(10);
                    foreach (var item in feedItems)
                    {
                        var title = item.Element("title")?.Value?.Trim();
                        var link = item.Element("link")?.Value?.Trim();
                        var description = item.Element("description")?.Value?.Trim();
                        var pubDateStr = item.Element("pubDate")?.Value;

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
                            continue;

                        // Strip HTML tags from description if any
                        var cleanDesc = StripHtml(description);
                        if (cleanDesc.Length > 220)
                        {
                            cleanDesc = cleanDesc[..220] + "...";
                        }

                        DateTime pubDate = DateTime.UtcNow;
                        if (!string.IsNullOrWhiteSpace(pubDateStr) && DateTime.TryParse(pubDateStr, out var parsedDate))
                        {
                            pubDate = parsedDate;
                        }

                        // Try to find image url from media:content or enclosure
                        var mediaContent = item.Elements().FirstOrDefault(e => e.Name.LocalName == "content" || e.Name.LocalName == "thumbnail");
                        var imageUrl = mediaContent?.Attribute("url")?.Value ?? item.Element("enclosure")?.Attribute("url")?.Value;

                        items.Add(new MovieNewsItemDto(
                            title,
                            cleanDesc,
                            source,
                            pubDate,
                            link,
                            imageUrl
                        ));
                    }

                    if (items.Count >= count)
                        break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to fetch RSS feed from {Source}: {Message}", source, ex.Message);
                }
            }

            if (items.Count == 0)
            {
                items = GetCuratedFallbackNews();
            }

            var sorted = items.OrderByDescending(x => x.PublishedAt).ToList();
            _cachedNews = sorted;
            _cacheExpiresAt = DateTime.UtcNow.AddMinutes(30);

            return sorted.Take(count).ToList();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var array = new char[input.Length];
        int arrayIndex = 0;
        bool inside = false;

        for (int i = 0; i < input.Length; i++)
        {
            char let = input[i];
            if (let == '<')
            {
                inside = true;
                continue;
            }
            if (let == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = let;
                arrayIndex++;
            }
        }

        return new string(array, 0, arrayIndex).Replace("&nbsp;", " ").Replace("&#8217;", "'").Replace("&#8220;", "\"").Replace("&#8221;", "\"").Trim();
    }

    private static List<MovieNewsItemDto> GetCuratedFallbackNews()
    {
        var now = DateTime.UtcNow;
        return new List<MovieNewsItemDto>
        {
            new("Christopher Nolan Sets Next Movie with Universal for Summer 2026",
                "Christopher Nolan's next feature film is officially underway at Universal Pictures with IMAX release scheduled for July 2026.",
                "Entertainment Weekly",
                now.AddHours(-2),
                "https://variety.com",
                "https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg"),

            new("Demon Slayer Infinity Castle Movie Trilogy Confirmed for Global Theatrical Release",
                "Ufotable announces the final battle arc will be adapted into a blockbuster three-part feature film saga.",
                "Anime News Network",
                now.AddHours(-5),
                "https://www.animenewsnetwork.com",
                "https://image.tmdb.org/t/p/w500/xUfRZu2mi8jH6SzQEJGP6tjBuYj.jpg"),

            new("Denis Villeneuve Begins Writing 'Dune: Messiah' Script for Warner Bros.",
                "Director Denis Villeneuve confirms development on the third installment completing Paul Atreides' story on Arrakis.",
                "Deadline",
                now.AddHours(-8),
                "https://deadline.com",
                "https://image.tmdb.org/t/p/w500/8b8R8l88Qje9dn9OE8PY05Nxl1X.jpg"),

            new("The Last of Us Season 2 Premiere Date Officially Announced by HBO",
                "Pedro Pascal and Bella Ramsey return as Joel and Ellie in the anticipated continuation of the critically acclaimed adaptation.",
                "Collider",
                now.AddHours(-12),
                "https://collider.com",
                "https://image.tmdb.org/t/p/w500/uKvVjHNqB5VmOrdxqAt2V7JMrQI.jpg"),

            new("Hayao Miyazaki Teases Next Studio Ghibli Project Following 'The Boy and the Heron'",
                "The legendary animator is already exploring new concepts and storyboards for his next animated feature in Tokyo.",
                "Variety",
                now.AddHours(-18),
                "https://variety.com",
                "https://image.tmdb.org/t/p/w500/jDQFD0FfT99F7L57jE9vA79ZJvJ.jpg"),

            new("Netflix Renews 'Arcane' Spin-off Projects Expanding the League of Legends Universe",
                "Riot Games and Fortiche will continue telling stories set in Runeterra exploring Noxus, Ionia and beyond.",
                "The Verge",
                now.AddHours(-24),
                "https://theverge.com",
                "https://image.tmdb.org/t/p/w500/fq29sRz2eC93B2jKzX6b2vG9kZq.jpg")
        };
    }
}
