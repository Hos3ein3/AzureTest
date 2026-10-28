using System.Net.Http;
using System.Threading.Tasks;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Trading.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AzureTest.UnitTest;

public class TradingQuoteTest
{
    [Fact]
    public async Task GetQuote_For_XANUSDT_MaintainsRealDexPriceAfterRefresh()
    {
        using var http = new HttpClient();
        var crypto = new CryptoMarketDataService(http, NullLogger<CryptoMarketDataService>.Instance);
        var forex = new ForexMarketDataService(http, NullLogger<ForexMarketDataService>.Instance);
        var service = new TradingMarketDataService(crypto, forex, NullLogger<TradingMarketDataService>.Instance, http);

        var quote = await service.GetQuoteAsync("XANUSDT");
        Assert.NotNull(quote);
        Assert.Equal("XANUSDT", quote.Symbol);
        Assert.True(quote.Price < 1.0m, $"Expected price to reflect DexScreener (~0.01), but was {quote.Price}");

        // Now trigger refresh
        await service.RefreshRealWorldDataAsync();

        var refreshedQuote = await service.GetQuoteAsync("XANUSDT");
        Assert.NotNull(refreshedQuote);
        Assert.True(refreshedQuote.Price < 1.0m, $"Price after refresh must stay at DEX level (~0.01), but was overwritten to {refreshedQuote.Price}");
    }
}
