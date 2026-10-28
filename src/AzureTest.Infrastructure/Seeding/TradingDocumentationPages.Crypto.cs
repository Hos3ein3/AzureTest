using System;
using AzureTest.Domain.Documents.Pages;
using AzureTest.Domain.Enums;

namespace AzureTest.Infrastructure.Seeding;

public static class CryptoDocumentationPageSeeder
{
    public static PageDocument GetPage()
    {
        return new PageDocument
        {
            Id = Guid.NewGuid(),
            Title = "Crypto Area Mastery Guide: Complete Features & Controller Operations",
            Slug = "how-to-use-crypto-trading",
            Status = PageStatus.Published,
            CreatedAt = new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc),
            Content = """
            <div class="py-6 px-4 max-w-5xl mx-auto font-sans leading-relaxed text-slate-800 dark:text-slate-200">
              <!-- Header Banner -->
              <div class="border-b border-slate-200 dark:border-slate-700 pb-6 mb-8">
                <div class="flex items-center gap-2 mb-3">
                  <span class="px-3 py-1 bg-amber-500/10 text-amber-600 dark:text-amber-400 font-bold text-xs rounded-full border border-amber-500/20">Official Platform Guide</span>
                  <span class="text-xs text-slate-500 dark:text-slate-400"><i class="fa-regular fa-clock mr-1"></i> B2 Level &bull; 16 min read</span>
                </div>
                <h1 class="text-3xl sm:text-4xl font-black text-slate-900 dark:text-white tracking-tight">Crypto Area Mastery Guide: Complete Features &amp; Controller Operations</h1>
                <p class="text-base text-slate-600 dark:text-slate-300 mt-2">A complete reference guide explaining how the Crypto Area works, detailing all eight controllers, their actions, on-chain intelligence, perpetual futures mechanics, order book ladders, smart order execution, arbitrage bots, and risk management.</p>
              </div>

              <!-- Quick Navigation Hub -->
              <div class="p-5 rounded-2xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 mb-8 space-y-3">
                <h3 class="text-xs font-black uppercase tracking-wider text-slate-400">Crypto Area Controller Sitemap</h3>
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs">
                  <a href="#sec-crypto-market" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-amber-600 dark:text-amber-400 font-bold hover:underline">1. CryptoMarket</a>
                  <a href="#sec-crypto-analysis" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-purple-600 dark:text-purple-400 font-bold hover:underline">2. CryptoAnalysis</a>
                  <a href="#sec-crypto-trade" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-emerald-600 dark:text-emerald-400 font-bold hover:underline">3. CryptoTrade</a>
                  <a href="#sec-crypto-portfolio" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-blue-600 dark:text-blue-400 font-bold hover:underline">4. CryptoPortfolio</a>
                  <a href="#sec-crypto-risk" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-rose-600 dark:text-rose-400 font-bold hover:underline">5. CryptoRisk</a>
                  <a href="#sec-crypto-strategy" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-indigo-600 dark:text-indigo-400 font-bold hover:underline">6. CryptoStrategy</a>
                  <a href="#sec-crypto-alerts" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-cyan-600 dark:text-cyan-400 font-bold hover:underline">7. CryptoAlerts</a>
                  <a href="#sec-crypto-settings" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 font-bold hover:underline">8. CryptoSettings</a>
                </div>
              </div>

              <div class="space-y-8 text-sm">
                <!-- Section 1: CryptoMarketController -->
                <div id="sec-crypto-market" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-coins text-amber-500"></i> 1. CryptoMarketController &mdash; Feeds, Order Depth, DEX &amp; Token Unlocks
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoMarket" class="text-amber-600 font-semibold underline">CryptoMarket</a> controller monitors real-time market data across both centralized and decentralized exchanges.
                  </p>

                  <div class="space-y-3">
                    <h4 class="font-bold text-xs uppercase tracking-wider text-slate-500">Actions &amp; Features:</h4>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                      <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                        <strong>Market Quotes &amp; Charting:</strong>
                        <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                          <li><strong>Index (<a href="/Crypto/CryptoMarket" class="text-amber-600 underline">/Index</a>):</strong> Live prices, 24h gainers/losers, volume, and market capitalization rankings.</li>
                          <li><strong>Chart (<a href="/Crypto/CryptoMarket/Chart" class="text-amber-600 underline">/Chart</a>):</strong> Broker-grade canvas workstation with indicators and live tick simulator.</li>
                          <li><strong>OrderBook (<a href="/Crypto/CryptoMarket/OrderBook" class="text-amber-600 underline">/OrderBook</a>):</strong> Live bids (green) vs asks (red), spread width, and recent tape trades.</li>
                        </ul>
                      </div>
                      <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                        <strong>Futures &amp; Order Flow Intelligence:</strong>
                        <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                          <li><strong>Futures (<a href="/Crypto/CryptoMarket/Futures" class="text-amber-600 underline">/Futures</a>):</strong> Perpetual funding rates (positive = longs pay shorts, negative = shorts pay longs) and Open Interest (OI).</li>
                          <li><strong>OrderFlow (<a href="/Crypto/CryptoMarket/OrderFlow" class="text-amber-600 underline">/OrderFlow</a>):</strong> Cumulative Volume Delta (CVD) tracking aggressive buyer vs seller market executions.</li>
                          <li><strong>DexScreener (<a href="/Crypto/CryptoMarket/DexScreener" class="text-amber-600 underline">/DexScreener</a>):</strong> Monitors on-chain liquidity pools across Uniswap, Raydium, and PancakeSwap.</li>
                          <li><strong>TokenUnlocks (<a href="/Crypto/CryptoMarket/TokenUnlocks" class="text-amber-600 underline">/TokenUnlocks</a>):</strong> Schedules cliff and linear vesting events to prepare for token supply dilution.</li>
                          <li><strong>ExchangeFlow (<a href="/Crypto/CryptoMarket/ExchangeFlow" class="text-amber-600 underline">/ExchangeFlow</a>):</strong> Tracks net coins entering or leaving centralized exchange wallets.</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Section 2: CryptoAnalysisController -->
                <div id="sec-crypto-analysis" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-brain text-purple-500"></i> 2. CryptoAnalysisController &mdash; Market Structure, Volume Profile &amp; Whale Tracking
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoAnalysis" class="text-purple-600 font-semibold underline">CryptoAnalysis</a> controller provides institutional market structure analysis.
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">Smart Money Concepts:</strong>
                      <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                        <li><strong>MarketStructure (<a href="/Crypto/CryptoAnalysis/MarketStructure" class="text-purple-600 underline">/MarketStructure</a>):</strong> Identifies Break of Structure (BOS) and Change of Character (CHoCH) trend shifts.</li>
                        <li><strong>VolumeProfile (<a href="/Crypto/CryptoAnalysis/VolumeProfile" class="text-purple-600 underline">/VolumeProfile</a>):</strong> Point of Control (POC), Value Area High (VAH), and Value Area Low (VAL).</li>
                        <li><strong>MarketRegime (<a href="/Crypto/CryptoAnalysis/MarketRegime" class="text-purple-600 underline">/MarketRegime</a>):</strong> Automatically detects if market is Trending, Ranging, or in High Volatility.</li>
                      </ul>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">Sentiment &amp; Screener:</strong>
                      <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                        <li><strong>Sentiment (<a href="/Crypto/CryptoAnalysis/Sentiment" class="text-purple-600 underline">/Sentiment</a>):</strong> Alternative.me Crypto Fear &amp; Greed Index (0-25 Extreme Fear, 75-100 Extreme Greed).</li>
                        <li><strong>Screener (<a href="/Crypto/CryptoAnalysis/Screener" class="text-purple-600 underline">/Screener</a>):</strong> Multi-criteria filter for coins with RSI &lt; 30, volume surges, or breakout volatility.</li>
                        <li><strong>OnChain (<a href="/Crypto/CryptoAnalysis/OnChain" class="text-purple-600 underline">/OnChain</a>):</strong> Whale wallet movements and reserve changes.</li>
                      </ul>
                    </div>
                  </div>
                </div>

                <!-- Section 3: CryptoTradeController -->
                <div id="sec-crypto-trade" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-bolt text-emerald-500"></i> 3. CryptoTradeController &mdash; Trade Terminal, Smart Orders &amp; Arbitrage Bots
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoTrade" class="text-emerald-600 font-semibold underline">CryptoTrade</a> controller powers paper trade execution with leverage up to 50x.
                  </p>

                  <div class="space-y-3 text-xs">
                    <ul class="list-disc pl-5 space-y-1.5 text-slate-600 dark:text-slate-400">
                      <li><strong>PlaceOrder:</strong> Opens Long or Short positions via Market execution, or sets pending Limit and Stop orders.</li>
                      <li><strong>ClosePosition:</strong> Market closes open position, returning locked margin and booking realized PnL.</li>
                      <li><strong>CancelOrder:</strong> Revokes an unfilled limit order.</li>
                      <li><strong>UpdateStops:</strong> Sets Stop Loss, Take Profit, and dynamic Trailing Stop distance.</li>
                      <li><strong>PlaceSmartOrder:</strong> Institutional order splitting (TWAP - Time Weighted Average Price) and Iceberg hidden orders to minimize price impact.</li>
                      <li><strong>Arbitrage &amp; ExecuteArbitrage (<a href="/Crypto/CryptoTrade/Arbitrage" class="text-emerald-600 underline">/Arbitrage</a>):</strong> Identifies cross-exchange price spreads (e.g., BTC cheaper on Binance than Kraken) and triangular arbitrage routes.</li>
                    </ul>
                  </div>
                </div>

                <!-- Section 4: CryptoPortfolioController -->
                <div id="sec-crypto-portfolio" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-wallet text-blue-500"></i> 4. CryptoPortfolioController &mdash; Balances, Journal &amp; Tax Accounting
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoPortfolio" class="text-blue-600 font-semibold underline">CryptoPortfolio</a> controller tracks account capital and fiscal compliance.
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 text-xs">
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
                      <strong class="text-slate-900 dark:text-white block font-bold">Portfolio (<a href="/Crypto/CryptoPortfolio" class="text-blue-600 underline">/Index</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">Asset allocation donut chart, daily equity growth, and ResetAccount button to restart at $10,000 USDT.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
                      <strong class="text-slate-900 dark:text-white block font-bold">Journal (<a href="/Crypto/CryptoPortfolio/Journal" class="text-blue-600 underline">/Journal</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">Track entry setups (Breakout, Retest, Dip) and emotional state for every trade.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
                      <strong class="text-slate-900 dark:text-white block font-bold">Tax Report (<a href="/Crypto/CryptoPortfolio/TaxReport" class="text-blue-600 underline">/TaxReport</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">Automated capital gains tax calculations using FIFO accounting, plus ExportTaxCsv download.</p>
                    </div>
                  </div>
                </div>

                <!-- Section 5: CryptoRiskController -->
                <div id="sec-crypto-risk" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-shield-halved text-rose-500"></i> 5. CryptoRiskController &mdash; Margin Health, Liquidation &amp; Kill Switch
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoRisk" class="text-rose-600 font-semibold underline">CryptoRisk</a> controller prevents catastrophic account drawdowns.
                  </p>

                  <ul class="list-disc pl-5 space-y-1.5 text-xs text-slate-600 dark:text-slate-400">
                    <li><strong>Margin Ratio &amp; Liquidation Price:</strong> Monitors proximity to liquidation (when unrealized losses equal initial margin).</li>
                    <li><strong>KillSwitch:</strong> Instant panic button liquidating all leveraged positions in seconds.</li>
                    <li><strong>UpdateLimits:</strong> Enforces daily loss limit percentages (default 3%) and maximum concurrent open positions.</li>
                    <li><strong>TopUpMargin &amp; TrimPosition:</strong> Interactively deposit emergency virtual margin or trim 25%/50% of position size to defend from liquidation.</li>
                    <li><strong>CalculateAtr:</strong> Computes Average True Range to set Stop Losses outside normal market volatility noise.</li>
                  </ul>
                </div>

                <!-- Section 6: CryptoStrategyController -->
                <div id="sec-crypto-strategy" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-flask text-indigo-500"></i> 6. CryptoStrategyController &mdash; Quantitative Strategy Backtesting
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoStrategy" class="text-indigo-600 font-semibold underline">CryptoStrategy</a> controller allows quantitative algorithmic testing.
                    Run historical simulations of <em>Trend Following</em>, <em>Grid Trading</em>, and <em>Mean Reversion</em> strategies with realistic exchange fee deductions (0.10% taker fee + 0.05% slippage modeling).
                  </p>
                </div>

                <!-- Section 7: CryptoAlertsController -->
                <div id="sec-crypto-alerts" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-bell text-cyan-500"></i> 7. CryptoAlertsController &mdash; Automated Price Trigger Alerts
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoAlerts" class="text-cyan-600 font-semibold underline">CryptoAlerts</a> controller manages automated price watchers. Create price crossing notifications (e.g., when BTC crosses above $70,000 or below $65,000), test delivery via SendTestAlert, and delete resolved alerts.
                  </p>
                </div>

                <!-- Section 8: CryptoSettingsController -->
                <div id="sec-crypto-settings" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-gears text-slate-500"></i> 8. CryptoSettingsController &mdash; Exchange API Keys &amp; WebSockets
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Crypto/CryptoSettings" class="text-slate-600 font-semibold underline">CryptoSettings</a> controller configures connections to public cryptocurrency data endpoints (Binance, CoinGecko, CoinLore, DIA). Includes a TestConnection ping tool to inspect API response times.
                  </p>
                </div>
              </div>
            </div>
            """
        };
    }
}
