using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Trading;

public class TradingMarketDataReliabilityTests
{
    private class MockHttpHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; set; } = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handler(request));
        }
    }

    [Fact]
    public async Task SearchAssetsAsync_ForSynUsdt_ReturnsAuthoritativeBinancePrice_AndNeverDexScamPrice()
    {
        // Arrange
        var mockCrypto = new Mock<ICryptoMarketDataService>();
        var mockForex = new Mock<IForexMarketDataService>();

        var mockHandler = new MockHttpHandler();
        mockHandler.Handler = req =>
        {
            var uri = req.RequestUri?.ToString() ?? "";

            // Binance returns official SYNUSDT price 0.17548
            if (uri.Contains("api.binance.com") && uri.Contains("SYNUSDT"))
            {
                var binanceJson = """
                {
                    "symbol": "SYNUSDT",
                    "priceChange": "0.00800000",
                    "priceChangePercent": "4.777",
                    "lastPrice": "0.17548000",
                    "highPrice": "0.18000000",
                    "lowPrice": "0.16500000",
                    "volume": "15420000.00000000"
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(binanceJson, Encoding.UTF8, "application/json")
                };
            }

            // DexScreener returns scam duplicate SYN token with 0.0000001456
            if (uri.Contains("api.dexscreener.com"))
            {
                var dexJson = """
                {
                    "pairs": [
                        {
                            "chainId": "base",
                            "pairAddress": "0x123456",
                            "baseToken": { "symbol": "SYN", "name": "SynapseOracle" },
                            "priceUsd": "0.0000001456",
                            "volume": { "h24": 500 },
                            "liquidity": { "usd": 120 }
                        }
                    ]
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(dexJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var httpClient = new HttpClient(mockHandler);
        var service = new TradingMarketDataService(
            mockCrypto.Object,
            mockForex.Object,
            NullLogger<TradingMarketDataService>.Instance,
            httpClient);

        // Act
        var results = await service.SearchAssetsAsync("SYNUSDT");
        var quote = await service.GetQuoteAsync("SYNUSDT");
        var livePrice = await service.GetCurrentPriceAsync("SYNUSDT");

        // Assert
        results.Should().NotBeEmpty();
        var synQuote = results.Should().ContainSingle(r => r.Symbol == "SYNUSDT").Subject;
        synQuote.Price.Should().Be(0.17548000m, "Should match authoritative Binance price");
        synQuote.FeedSource.Should().Be("binance");

        quote.Should().NotBeNull();
        quote!.Price.Should().Be(0.17548000m);
        livePrice.Should().Be(0.17548000m);

        // Guarantee price was NEVER corrupted to the 0.00000015 scam price
        livePrice.Should().NotBe(0.0000001456m);
    }

    [Fact]
    public async Task SearchAssetsAsync_ForMexcToken_ReturnsAuthoritativeMexcPrice_WhenBinanceReturns400()
    {
        // Arrange
        var mockCrypto = new Mock<ICryptoMarketDataService>();
        var mockForex = new Mock<IForexMarketDataService>();

        var mockHandler = new MockHttpHandler();
        mockHandler.Handler = req =>
        {
            var uri = req.RequestUri?.ToString() ?? "";

            // Binance does NOT have AGTUSDT
            if (uri.Contains("api.binance.com") && uri.Contains("AGTUSDT"))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            // MEXC has AGTUSDT with official price 0.044600
            if (uri.Contains("api.mexc.com") && uri.Contains("AGTUSDT"))
            {
                var mexcJson = """
                {
                    "symbol": "AGTUSDT",
                    "priceChange": "-0.001500",
                    "priceChangePercent": "-0.0325",
                    "lastPrice": "0.044600",
                    "highPrice": "0.048000",
                    "lowPrice": "0.042000",
                    "volume": "850000"
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(mexcJson, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var httpClient = new HttpClient(mockHandler);
        var service = new TradingMarketDataService(
            mockCrypto.Object,
            mockForex.Object,
            NullLogger<TradingMarketDataService>.Instance,
            httpClient);

        // Act
        var results = await service.SearchAssetsAsync("AGTUSDT");
        var quote = await service.GetQuoteAsync("AGTUSDT");

        // Assert
        results.Should().NotBeEmpty();
        var agtQuote = results.Should().ContainSingle(r => r.Symbol == "AGTUSDT").Subject;
        agtQuote.Price.Should().Be(0.044600m);
        agtQuote.FeedSource.Should().Be("mexc");

        quote.Should().NotBeNull();
        quote!.Price.Should().Be(0.044600m);
    }

    [Fact]
    public async Task RefreshSymbolsDataAsync_OnlyRequestsSpecifiedOpenSymbols_NotAllCryptos()
    {
        // Arrange
        var mockCrypto = new Mock<ICryptoMarketDataService>();
        var mockForex = new Mock<IForexMarketDataService>();

        var requestedUris = new List<string>();
        var mockHandler = new MockHttpHandler();
        mockHandler.Handler = req =>
        {
            var uri = req.RequestUri?.ToString() ?? "";
            requestedUris.Add(uri);

            if (uri.Contains("SOLUSDT"))
            {
                var json = """
                {
                    "symbol": "SOLUSDT",
                    "priceChange": "1.50000000",
                    "priceChangePercent": "1.450",
                    "lastPrice": "103.50000000",
                    "highPrice": "105.00000000",
                    "lowPrice": "99.00000000",
                    "volume": "100000.00000000"
                }
                """;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var service = new TradingMarketDataService(
            mockCrypto.Object,
            mockForex.Object,
            NullLogger<TradingMarketDataService>.Instance,
            new HttpClient(mockHandler));

        // Act: Refresh only SOLUSDT (the single open position)
        await service.RefreshSymbolsDataAsync(new[] { "SOLUSDT" });
        var price = await service.GetCurrentPriceAsync("SOLUSDT");

        // Assert
        price.Should().Be(103.50000000m);
        requestedUris.Should().ContainSingle();
        requestedUris[0].Should().Contain("symbol=SOLUSDT");
        requestedUris[0].Should().NotContain("BTCUSDT", "Should only query the open position symbol");
    }

    [Fact]
    public async Task CryptoMarketDataService_GetCurrentPriceAsync_RetriesOnRateLimit_AndPreservesLastKnownPrice()
    {
        // Arrange
        int callCount = 0;
        var mockHandler = new MockHttpHandler();
        mockHandler.Handler = req =>
        {
            callCount++;
            if (callCount == 1)
            {
                // First call: transient 429 rate limit
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            }

            // Second call: succeeds with real live price
            var json = """{"symbol": "SOLUSDT", "price": "102.85"}""";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        };

        var cryptoService = new CryptoMarketDataService(
            new HttpClient(mockHandler),
            NullLogger<CryptoMarketDataService>.Instance);

        // Act
        var price = await cryptoService.GetCurrentPriceAsync("SOLUSDT");

        // Assert
        callCount.Should().Be(2, "Should retry and succeed on second attempt after rate limit");
        price.Should().Be(102.85m);
        price.Should().NotBe(165m, "Should NEVER return the old hardcoded 165m value");
    }
}
