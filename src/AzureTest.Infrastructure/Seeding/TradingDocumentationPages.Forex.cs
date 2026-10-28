using System;
using AzureTest.Domain.Documents.Pages;
using AzureTest.Domain.Enums;

namespace AzureTest.Infrastructure.Seeding;

public static class ForexDocumentationPageSeeder
{
    public static PageDocument GetPage()
    {
        return new PageDocument
        {
            Id = Guid.NewGuid(),
            Title = "Forex Area Mastery Guide: Complete Features & Controller Operations",
            Slug = "how-to-use-forex-trading",
            Status = PageStatus.Published,
            CreatedAt = new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc),
            Content = """
            <div class="py-6 px-4 max-w-5xl mx-auto font-sans leading-relaxed text-slate-800 dark:text-slate-200">
              <!-- Header Banner -->
              <div class="border-b border-slate-200 dark:border-slate-700 pb-6 mb-8">
                <div class="flex items-center gap-2 mb-3">
                  <span class="px-3 py-1 bg-blue-500/10 text-blue-600 dark:text-blue-400 font-bold text-xs rounded-full border border-blue-500/20">Official Platform Guide</span>
                  <span class="text-xs text-slate-500 dark:text-slate-400"><i class="fa-regular fa-clock mr-1"></i> B2 Level &bull; 14 min read</span>
                </div>
                <h1 class="text-3xl sm:text-4xl font-black text-slate-900 dark:text-white tracking-tight">Forex Area Mastery Guide: Complete Features &amp; Controller Operations</h1>
                <p class="text-base text-slate-600 dark:text-slate-300 mt-2">A comprehensive reference guide explaining how the Forex Area works, detailing all seven controllers, their user actions, pip math, technical indicators, economic fundamentals, and risk-managed demo execution.</p>
              </div>

              <!-- Quick Navigation Hub -->
              <div class="p-5 rounded-2xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 mb-8 space-y-3">
                <h3 class="text-xs font-black uppercase tracking-wider text-slate-400">Forex Area Controller Sitemap</h3>
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs">
                  <a href="#sec-forex-market" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-blue-600 dark:text-blue-400 font-bold hover:underline">1. ForexMarket</a>
                  <a href="#sec-forex-analysis" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-indigo-600 dark:text-indigo-400 font-bold hover:underline">2. ForexAnalysis</a>
                  <a href="#sec-forex-trade" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-emerald-600 dark:text-emerald-400 font-bold hover:underline">3. ForexTrade</a>
                  <a href="#sec-forex-risk" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-rose-600 dark:text-rose-400 font-bold hover:underline">4. ForexRisk</a>
                  <a href="#sec-forex-strategy" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-purple-600 dark:text-purple-400 font-bold hover:underline">5. ForexStrategy</a>
                  <a href="#sec-forex-journal" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-amber-600 dark:text-amber-400 font-bold hover:underline">6. ForexJournal</a>
                  <a href="#sec-forex-settings" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-teal-600 dark:text-teal-400 font-bold hover:underline">7. ForexSettings</a>
                  <a href="#sec-forex-routine" class="p-2 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 font-bold hover:underline">8. Daily Routine</a>
                </div>
              </div>

              <div class="space-y-8 text-sm">
                <!-- Section 1: ForexMarketController -->
                <div id="sec-forex-market" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-chart-line text-blue-500"></i> 1. ForexMarketController &mdash; Rates, Pro Charts &amp; Global Sessions
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexMarket" class="text-blue-600 font-semibold underline">ForexMarket</a> controller serves as the primary data hub for currency pairs. Foreign exchange involves quoting one currency against another (e.g., EUR/USD: Base is Euro, Quote is US Dollar).
                  </p>

                  <div class="space-y-3">
                    <h4 class="font-bold text-xs uppercase tracking-wider text-slate-500">Actions &amp; Features:</h4>
                    <ul class="list-disc pl-5 space-y-2 text-xs text-slate-600 dark:text-slate-400">
                      <li>
                        <strong>Index (<a href="/Forex/ForexMarket" class="text-blue-600 underline">/Forex/ForexMarket</a>):</strong> Displays real-time Bid/Ask rates, spreads, and high/low values for all major, minor, and exotic pairs. Includes search filtering and an automatic Pip Value Calculator (e.g., 1 Standard Lot of EUR/USD equals $10.00 per pip).
                      </li>
                      <li>
                        <strong>Chart (<a href="/Forex/ForexMarket/Chart" class="text-blue-600 underline">/Forex/ForexMarket/Chart</a>):</strong> High-performance HTML5 Canvas workstation. Supports 6 candle styles (Candlesticks, Hollow Candles, Heikin-Ashi, OHLC Bars, Line, Area), 5 technical overlays (EMA 20, EMA 50, SMA 200, Bollinger Bands, Volume), on-chart drawing tools, and a Live Tick Simulator to test execution in quiet market hours.
                      </li>
                      <li>
                        <strong>Sessions (<a href="/Forex/ForexMarket/Sessions" class="text-blue-600 underline">/Forex/ForexMarket/Sessions</a>):</strong> Live clocks for the four major financial hubs: Sydney, Tokyo, London, and New York. Highlights the high-volatility <em>London/New York Overlap</em> (13:00&ndash;16:00 UTC), when pip moves and liquidity are at their peak.
                      </li>
                    </ul>
                  </div>
                </div>

                <!-- Section 2: ForexAnalysisController -->
                <div id="sec-forex-analysis" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-compass-drafting text-indigo-500"></i> 2. ForexAnalysisController &mdash; Technical Indicators &amp; Macro Intelligence
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexAnalysis" class="text-indigo-600 font-semibold underline">ForexAnalysis</a> controller synthesizes mathematical chart patterns with macroeconomic fundamentals.
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1.5">
                      <strong class="text-slate-900 dark:text-white block font-bold">Technical (<a href="/Forex/ForexAnalysis/Technical" class="text-indigo-600 underline">/Technical</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        Evaluates multi-timeframe indicators: RSI (14) overbought/oversold, MACD signal crossovers, and dynamic Pivot Points computed in four standard methodologies: Classic, Fibonacci, Camarilla, and Woodie.
                      </p>
                    </div>
                    <div class="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1.5">
                      <strong class="text-slate-900 dark:text-white block font-bold">Currency Strength (<a href="/Forex/ForexAnalysis/Strength" class="text-indigo-600 underline">/Strength</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        Calculates an aggregate momentum score (from 0 to 10) for the 8 major currencies: USD, EUR, GBP, JPY, CHF, CAD, AUD, and NZD. The golden rule is: buy the strongest currency against the weakest currency.
                      </p>
                    </div>
                  </div>

                  <div class="p-3 rounded-xl bg-indigo-50/50 dark:bg-indigo-950/20 border border-indigo-200 dark:border-indigo-800/40 text-xs text-indigo-900 dark:text-indigo-300">
                    <strong>Fundamental (<a href="/Forex/ForexAnalysis/Fundamental" class="underline font-bold">/Fundamental</a>):</strong>
                    Tracks the Economic Calendar, filtering high-impact announcements (Tier-1 news like US Non-Farm Payrolls, CPI Inflation, and central bank interest rate decisions by the Federal Reserve and European Central Bank).
                  </div>
                </div>

                <!-- Section 3: ForexTradeController -->
                <div id="sec-forex-trade" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-bolt text-emerald-500"></i> 3. ForexTradeController &mdash; Demo Terminal, Advanced Orders &amp; Webhooks
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexTrade" class="text-emerald-600 font-semibold underline">ForexTrade</a> controller handles trade execution, live risk protection, and automation.
                  </p>

                  <div class="space-y-3">
                    <h4 class="font-bold text-xs uppercase tracking-wider text-slate-500">Complete Action Inventory:</h4>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                      <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                        <span class="font-bold text-slate-900 dark:text-white block">Order Execution:</span>
                        <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                          <li><strong>PlaceOrder:</strong> Opens instant Market execution or sets pending Buy/Sell Limit or Stop entries.</li>
                          <li><strong>Close:</strong> Closes an active position and applies net profit/loss to cash balance.</li>
                          <li><strong>Cancel:</strong> Revokes an unfilled pending order.</li>
                          <li><strong>ResetAccount:</strong> Resets demo equity and margin to initial $10,000.</li>
                        </ul>
                      </div>
                      <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                        <span class="font-bold text-slate-900 dark:text-white block">In-Trade Risk Control:</span>
                        <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                          <li><strong>PartialClose:</strong> Closes a portion of the lot size (e.g. 0.50 of 1.00 lot) to secure guaranteed profits while letting runners ride.</li>
                          <li><strong>MoveBreakEven:</strong> Moves the Stop Loss directly to entry price to eliminate downside risk.</li>
                          <li><strong>UpdateStops:</strong> Modifies active Stop Loss and Take Profit levels.</li>
                          <li><strong>KillSwitch:</strong> Emergency one-click button to liquidate all positions immediately.</li>
                        </ul>
                      </div>
                    </div>

                    <div class="p-3 rounded-xl bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs">
                      <strong>TradingView Webhook Integration (<code>/Forex/ForexTrade/Webhook</code>):</strong>
                      Allows external automated systems or PineScript alerts to send JSON payloads (<code>{ "symbol": "EURUSD", "action": "BUY", "lotSize": 0.5, "stopLoss": 1.0800 }</code>) to execute trades autonomously without manual intervention.
                    </div>
                  </div>
                </div>

                <!-- Section 4: ForexRiskController -->
                <div id="sec-forex-risk" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-shield-halved text-rose-500"></i> 4. ForexRiskController &mdash; ESMA Compliance &amp; Lot Sizing Calculator
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexRisk" class="text-rose-600 font-semibold underline">ForexRisk</a> controller enforces institutional-grade capital protection based on regulatory frameworks.
                  </p>

                  <ul class="list-disc pl-5 space-y-2 text-xs text-slate-600 dark:text-slate-400">
                    <li>
                      <strong>ESMA Leverage Guard:</strong> Implements the European Securities and Markets Authority limits (maximum 30:1 for major currency pairs and 20:1 for non-major pairs). This prevents retail accounts from blowing up due to excessive borrowing.
                    </li>
                    <li>
                      <strong>The 1% Rule Calculator (<code>/Forex/ForexRisk/Calculate</code>):</strong> Takes your account balance, chosen risk percentage (e.g. 1.0%), and stop loss pips, and calculates the mathematically safe lot size:
                      <br><code class="block p-2 rounded bg-slate-100 dark:bg-slate-900 mt-1 font-mono text-3xs">Lot Size = (Account Balance &times; Risk %) &divide; (Stop Loss Pips &times; Pip Value per Lot)</code>
                    </li>
                  </ul>
                </div>

                <!-- Section 5: ForexStrategyController -->
                <div id="sec-forex-strategy" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-microchip text-purple-500"></i> 5. ForexStrategyController &mdash; Quantitative Backtest Lab
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexStrategy" class="text-purple-600 font-semibold underline">ForexStrategy</a> controller tests trading rules over historical candles before putting real money on the line.
                  </p>

                  <div class="space-y-2 text-xs text-slate-600 dark:text-slate-400">
                    <p>
                      <strong>Execute &amp; Results (<a href="/Forex/ForexStrategy" class="text-purple-600 underline">/Index</a> &amp; <code>/Results/{id}</code>):</strong>
                      Simulates rule-based strategies (such as <em>EMA 20/50 Crossover</em>, <em>RSI Mean Reversion</em>, and <em>London Breakout</em>) across historical data. Reports key statistics:
                    </p>
                    <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 font-mono text-3xs">
                      <div class="p-2 rounded bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">Win Rate %</div>
                      <div class="p-2 rounded bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">Profit Factor (&gt; 1.5)</div>
                      <div class="p-2 rounded bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">Max Drawdown %</div>
                      <div class="p-2 rounded bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800">Sharpe Ratio</div>
                    </div>
                  </div>
                </div>

                <!-- Section 6: ForexJournalController -->
                <div id="sec-forex-journal" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-book-journal-whills text-amber-500"></i> 6. ForexJournalController &mdash; Psychology, Screenshots &amp; Tax Export
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexJournal" class="text-amber-600 font-semibold underline">ForexJournal</a> controller addresses trader psychology and compliance record-keeping.
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">Psychology &amp; Direct Image Upload:</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        Record emotional states for every setup (Disciplined, FOMO, Anxious, Greedy). Upload chart screenshots directly from your computer to store evidence of technical execution.
                      </p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">ExportTax (<a href="/Forex/ForexJournal/ExportTax" class="text-amber-600 underline">/ExportTax</a>):</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        Generates an annual profit and loss ledger for fiscal authorities, calculating gross profit, broker commissions, and net capital gain in compliant CSV format.
                      </p>
                    </div>
                  </div>
                </div>

                <!-- Section 7: ForexSettingsController -->
                <div id="sec-forex-settings" class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-sliders text-teal-500"></i> 7. ForexSettingsController &mdash; API Provider Configuration
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The <a href="/Forex/ForexSettings" class="text-teal-600 font-semibold underline">ForexSettings</a> controller configures external data providers (Alpha Vantage, ExchangeRate-API, Finnhub). Allows users to store private API keys, test network latencies, and configure local cache TTLs to avoid hitting third-party rate limits.
                  </p>
                </div>

                <!-- Section 8: Step-by-Step Daily Routine -->
                <div id="sec-forex-routine" class="p-6 rounded-2xl bg-blue-50/50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-800/40 space-y-3">
                  <h3 class="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-list-check text-blue-600"></i> Professional Daily Routine for Forex Traders (B2 Checklist)
                  </h3>
                  <ol class="list-decimal pl-5 space-y-1.5 text-xs text-slate-700 dark:text-slate-300">
                    <li><strong>07:30 UTC:</strong> Check the <a href="/Forex/ForexAnalysis/Fundamental" class="font-bold underline">Economic Calendar</a>. Note down any High-Impact news events for EUR, GBP, or USD.</li>
                    <li><strong>08:00 UTC (London Open):</strong> Open <a href="/Forex/ForexMarket/Sessions" class="font-bold underline">Market Sessions</a> and review currency volume. Check the <a href="/Forex/ForexAnalysis/Strength" class="font-bold underline">Currency Strength Meter</a>.</li>
                    <li><strong>08:15 UTC:</strong> Open the <a href="/Forex/ForexMarket/Chart" class="font-bold underline">Pro Chart</a>. Draw support/resistance levels and review daily Pivot Points.</li>
                    <li><strong>Order Sizing:</strong> Use the <a href="/Forex/ForexRisk" class="font-bold underline">Risk Calculator</a> to determine exact lot size ensuring risk does not exceed 1% of account equity.</li>
                    <li><strong>Post-Trade:</strong> Move Stop Loss to Break-Even once price reaches 1R profit, and log execution in the <a href="/Forex/ForexJournal" class="font-bold underline">Trade Journal</a>.</li>
                  </ol>
                </div>
              </div>
            </div>
            """
        };
    }
}
