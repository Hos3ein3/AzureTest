using System;
using AzureTest.Domain.Documents.Pages;
using AzureTest.Domain.Enums;

namespace AzureTest.Infrastructure.Seeding;

public static class UniversalTradingDocumentationPageSeeder
{
    public static PageDocument GetPage()
    {
        return new PageDocument
        {
            Id = Guid.NewGuid(),
            Title = "How to Use the Universal Simplified Paper Trading Platform",
            Slug = "simplified-paper-trading-guide",
            Status = PageStatus.Published,
            CreatedAt = new DateTime(2026, 10, 27, 18, 0, 0, DateTimeKind.Utc),
            Content = """
            <div class="py-6 px-4 max-w-5xl mx-auto font-sans leading-relaxed text-slate-800 dark:text-slate-200">
              <!-- Header Banner -->
              <div class="border-b border-slate-200 dark:border-slate-700 pb-6 mb-8">
                <div class="flex items-center gap-2 mb-3">
                  <span class="px-3 py-1 bg-brand-500/10 text-brand-600 dark:text-brand-400 font-bold text-xs rounded-full border border-brand-500/20">Official Platform Guide</span>
                  <span class="text-xs text-slate-500 dark:text-slate-400"><i class="fa-regular fa-clock mr-1"></i> B2 Level &bull; 15 min read</span>
                </div>
                <h1 class="text-3xl sm:text-4xl font-black text-slate-900 dark:text-white tracking-tight">How to Use the Universal Simplified Paper Trading Platform</h1>
                <p class="text-base text-slate-600 dark:text-slate-300 mt-2">A step-by-step master user guide for our beginner-friendly yet institutional-grade paper trading platform. Learn how to search multi-asset markets, draw professional chart patterns, toggle simplified terminology, manage risk, execute programmatic JSON actions, and let Hangfire automatically protect your open trades.</p>
              </div>

              <!-- Quick Highlights Cards -->
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                <div class="p-4 rounded-2xl bg-brand-50/50 dark:bg-brand-950/20 border border-brand-200 dark:border-brand-800/40">
                  <div class="text-xs font-bold text-brand-600 dark:text-brand-400 uppercase tracking-wide">Multi-Asset Hub</div>
                  <div class="text-base font-bold text-slate-900 dark:text-white mt-1"><i class="fa-solid fa-layer-group text-brand-500 mr-2"></i>5 Global Markets</div>
                  <p class="text-xs text-slate-600 dark:text-slate-400 mt-1">Practice trading Cryptocurrencies, Forex currencies, US Stocks, Global Indices, and Commodity Futures in one single terminal.</p>
                </div>
                <div class="p-4 rounded-2xl bg-emerald-50/50 dark:bg-emerald-950/20 border border-emerald-200 dark:border-emerald-800/40">
                  <div class="text-xs font-bold text-emerald-600 dark:text-emerald-400 uppercase tracking-wide">Plain English Switch</div>
                  <div class="text-base font-bold text-slate-900 dark:text-white mt-1"><i class="fa-solid fa-toggle-on text-emerald-500 mr-2"></i>No Confusing Buzzwords</div>
                  <p class="text-xs text-slate-600 dark:text-slate-400 mt-1">One-click toggle switches every complex Wall Street term (margin, ticks, TIF) into plain, simple English words anyone can understand.</p>
                </div>
                <div class="p-4 rounded-2xl bg-purple-50/50 dark:bg-purple-950/20 border border-purple-200 dark:border-purple-800/40">
                  <div class="text-xs font-bold text-purple-600 dark:text-purple-400 uppercase tracking-wide">Hangfire Autopilot</div>
                  <div class="text-base font-bold text-slate-900 dark:text-white mt-1"><i class="fa-solid fa-robot text-purple-500 mr-2"></i>Automated SL / TP Exits</div>
                  <p class="text-xs text-slate-600 dark:text-slate-400 mt-1">A background Hangfire worker checks prices every minute, automatically triggering Stop Loss and Take Profit orders while you sleep.</p>
                </div>
              </div>

              <!-- Main Content Sections -->
              <div class="space-y-8 text-sm">
                <!-- Section 1: Multi-Asset Explorer & Watchlist -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-magnifying-glass text-brand-500"></i> 1. Finding Assets &amp; Managing Your Watchlist
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    The platform aggregates five major global financial asset classes in a single unified directory:
                  </p>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">Supported Asset Classes:</strong>
                      <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                        <li><strong>Cryptocurrencies:</strong> Bitcoin (BTC), Ethereum (ETH), Solana (SOL), BNB, XRP, Dogecoin.</li>
                        <li><strong>Forex Currencies:</strong> EUR/USD, GBP/USD, USD/JPY, AUD/USD, USD/CAD, USD/CHF.</li>
                        <li><strong>Equities / Stocks:</strong> Apple (AAPL), Microsoft (MSFT), NVIDIA (NVDA), Tesla (TSLA), Amazon (AMZN).</li>
                        <li><strong>Stock Indices:</strong> S&amp;P 500 (SPX), NASDAQ 100 (NDX), Dow Jones (DJI), Germany DAX 40.</li>
                        <li><strong>Commodity Futures:</strong> Gold (XAUUSD), Silver (XAGUSD), Crude Oil (USOIL), Natural Gas.</li>
                      </ul>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-slate-900 dark:text-white block font-bold">Search &amp; Watchlist Features:</strong>
                      <ul class="list-disc pl-4 space-y-1 text-slate-600 dark:text-slate-400">
                        <li><strong>Instant Search:</strong> Type any ticker symbol or full company name into the search bar to filter instantly.</li>
                        <li><strong>Category Pills:</strong> Click <em>Crypto</em>, <em>Forex</em>, <em>Stocks</em>, or <em>Futures</em> to view only that market.</li>
                        <li><strong>Pin with One Click:</strong> Click the gold star (<i class="fa-solid fa-star text-amber-500"></i>) beside any asset to add it to your personal pinned Watchlist.</li>
                        <li><strong>Filter Watchlist:</strong> Click the <strong>Pinned</strong> button to view only the assets you are actively monitoring.</li>
                      </ul>
                    </div>
                  </div>
                </div>

                <!-- Section 2: Charting & All 8 Drawing Categories -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-chart-candlestick text-indigo-500"></i> 2. TradingView-Style Interactive Chart &amp; The 8 Drawing Tool Suites
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    At the top of the trading terminal sits our full-featured Canvas chart engine. It supports multiple timeframes (1m, 5m, 15m, 1H, 4H, 1D), Candlestick or Line styles, and an exhaustive suite of professional technical analysis tools organized into eight categories:
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-sky-600 dark:text-sky-400 block font-bold">1. Lines &amp; Channels:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Trend Line, Ray, Info Line, Extended Line, Trend Angle, Horizontal Line, Horizontal Ray, Vertical Line, Cross Line, Parallel Channel, Regression Trend, Flat Top/Bottom, Disjoint Channel, Pitchfork, Schiff Pitchfork, Modified Schiff, and Inside Pitchfork.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-amber-600 dark:text-amber-400 block font-bold">2. Gann &amp; Fibonacci:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Fib Retracement (23.6%, 38.2%, 50%, 61.8%), Trend-Based Fib, Fib Channel, Fib Time Zone, Fib Speed Resistance, Fib Circle, Fib Spiral, Fib Wedge, Pitchfan, Gann Box, Gann Square Fixed, and Gann Fan.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-emerald-600 dark:text-emerald-400 block font-bold">3. Geometric Patterns:</strong>
                      <p class="text-slate-600 dark:text-slate-400">XABCD Pattern, Cypher Pattern, Head and Shoulders, ABCD Pattern, Triangle Pattern, Three Drives, Elliott Impulse Wave (12345), Elliott Correction (ABC), Elliott Triangle, Elliott Double Combo, Elliott Triple Combo, Cyclic Lines, Time Cycles, and Sine Line.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-indigo-600 dark:text-indigo-400 block font-bold">4. Forecasting &amp; Measurement:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Long Position (Risk:Reward Box), Short Position (Risk:Reward Box), Forecast, Bars Pattern Cloner, Ghost Feed, Projection, Anchored VWAP, Fixed Range Volume, Anchored Volume Profile, Price Range Ruler, Date Range, and Date/Price Range Box.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-purple-600 dark:text-purple-400 block font-bold">5. Geometric Shapes:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Brush Freehand, Highlighter, Arrow Marker, Arrow, Arrow Marker Up, Arrow Marker Down, Rectangle (Supply/Demand Zones), Rotated Rectangle, Path Polycurve, Circle, Ellipse, Polyline, Triangle, Arc, Curve, and Double Curve.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-pink-600 dark:text-pink-400 block font-bold">6. Annotations:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Text Label, Anchored Text, Sticky Note, Price Note, Pin Marker, Table Data, Callout Bubble, Comment Flag, Price Label Marker, Signpost Marker, Flag Mark, Image Stamp, Tweet Badge, and Idea Lightbulb.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-amber-500 block font-bold">7. Visuals &amp; Emojis:</strong>
                      <p class="text-slate-600 dark:text-slate-400">One-click emoji stamps (🚀, 🔥, 💎, 🎯, ⚠️, 📈, 📉, 🛑, ⭐, 💰) directly placed on chart pivots.</p>
                    </div>
                    <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1">
                      <strong class="text-blue-500 block font-bold">8. High-Res Snapshot:</strong>
                      <p class="text-slate-600 dark:text-slate-400">Click <strong>Snapshot</strong> to export a clean PNG image of your annotated chart to share with your trading community or journal.</p>
                    </div>
                  </div>
                </div>

                <!-- Section 3: The Plain Words vs Buzzwords Switch -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-toggle-on text-emerald-500"></i> 3. The UIComponent Switch: Plain English vs. Professional Terms
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    Traditional trading platforms use intimidating financial jargon. We solved this with an interactive toggle switch right above the order ticket. When enabled, every label switches into simple everyday English:
                  </p>

                  <div class="overflow-x-auto">
                    <table class="w-full text-left text-xs border border-slate-200 dark:border-slate-700 rounded-xl overflow-hidden">
                      <thead class="bg-slate-50 dark:bg-slate-900 text-slate-700 dark:text-slate-300 font-bold">
                        <tr class="border-b border-slate-200 dark:border-slate-700">
                          <th class="p-2.5">Trading Concept</th>
                          <th class="p-2.5 text-rose-600 dark:text-rose-400">Wall Street Buzzwords</th>
                          <th class="p-2.5 text-emerald-600 dark:text-emerald-400 font-bold">Plain English Mode (Recommended)</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-slate-100 dark:divide-slate-800 text-slate-600 dark:text-slate-400">
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Direction</td>
                          <td class="p-2.5 font-mono">Long / Short Position</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">Buy (Expect Rise) / Sell (Expect Fall)</td>
                        </tr>
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Entry Price</td>
                          <td class="p-2.5 font-mono">Limit / Fill Quote</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">Buy / Target Price</td>
                        </tr>
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Capital Sizing</td>
                          <td class="p-2.5 font-mono">% Balance / USD Margin</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">% of My Money / Cash Required ($)</td>
                        </tr>
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Downside Risk</td>
                          <td class="p-2.5 font-mono">Stop Loss Ticks / Risk % Balance</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">Safety Net / Max Loss ($ and %)</td>
                        </tr>
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Upside Target</td>
                          <td class="p-2.5 font-mono">Take Profit Price / Reward USD</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">Profit Target / Target Gain ($ and %)</td>
                        </tr>
                        <tr>
                          <td class="p-2.5 font-bold text-slate-800 dark:text-slate-200">Order Lifespan</td>
                          <td class="p-2.5 font-mono">Time In Force (TIF, GTC, GTD)</td>
                          <td class="p-2.5 font-semibold text-emerald-600 dark:text-emerald-400">Order Duration (Day, Week, Month, Date)</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>

                <!-- Section 4: UIComponent Tab Group: Market, Limit, Stop -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-layer-group text-blue-500"></i> 4. Order Execution Tabs: Market, Limit, and Stop
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    Our UIComponent group tab divides trading into three distinct order methods:
                  </p>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
                    <div class="p-4 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-2">
                      <strong class="text-brand-600 dark:text-brand-400 block font-bold text-sm">1. Market Order (Instant)</strong>
                      <p class="text-slate-600 dark:text-slate-400">Executes immediately at the best available current market price. Use this when you want to get into the trade right now without waiting.</p>
                      <ul class="list-disc pl-4 space-y-1 text-slate-500 text-3xs">
                        <li>Calculates % balance and USD margin in real time.</li>
                        <li>Calculates estimated Risk USD and Reward USD.</li>
                      </ul>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-2">
                      <strong class="text-indigo-600 dark:text-indigo-400 block font-bold text-sm">2. Limit Order (Discount)</strong>
                      <p class="text-slate-600 dark:text-slate-400">Waits for price to pull back to a better level. Buy Limit sits <em>below</em> market price; Sell Limit sits <em>above</em> market price.</p>
                      <ul class="list-disc pl-4 space-y-1 text-slate-500 text-3xs">
                        <li>Adds <strong>Limit Execution Price</strong> input.</li>
                        <li>Adds <strong>Time in Force</strong> selector (Day, Week, Month, GTD).</li>
                      </ul>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-2">
                      <strong class="text-purple-600 dark:text-purple-400 block font-bold text-sm">3. Stop Order (Breakout)</strong>
                      <p class="text-slate-600 dark:text-slate-400">Triggers an order only after price breaks through a key resistance or support level. Ideal for momentum breakouts.</p>
                      <ul class="list-disc pl-4 space-y-1 text-slate-500 text-3xs">
                        <li>Adds <strong>Stop Activation Price</strong> input.</li>
                        <li>Includes full protective bracket and TIF options.</li>
                      </ul>
                    </div>
                  </div>
                </div>

                <!-- Section 5: Multi-Level Take Profit & Risk Management -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-bullseye text-emerald-500"></i> 5. Multi-Level Take Profit (TP1, TP2, TP3) &amp; Trailing Stops
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    Professional traders never close an entire trade all at once. Our exit bracket allows scaling out:
                  </p>

                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                    <div class="p-3.5 rounded-xl bg-emerald-50/50 dark:bg-emerald-950/20 border border-emerald-200 dark:border-emerald-800/40 space-y-1.5">
                      <strong class="text-emerald-800 dark:text-emerald-300 font-bold block">Multiple TP Scaling (TP1, TP2, TP3):</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        &bull; <strong>TP1 (Primary Target):</strong> Closes 50% of the position size to lock in profit.
                        <br>&bull; <strong>TP2 (Mid Target):</strong> Closes 30% of the position as trend extends.
                        <br>&bull; <strong>TP3 (Moonbag Runner):</strong> Lets the remaining 20% ride to maximize long-term trend gains.
                      </p>
                    </div>
                    <div class="p-3.5 rounded-xl bg-rose-50/50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-800/40 space-y-1.5">
                      <strong class="text-rose-800 dark:text-rose-300 font-bold block">Trailing Stop Protection:</strong>
                      <p class="text-slate-600 dark:text-slate-400">
                        Enter a Trailing Stop distance (e.g. $50.00). As price climbs in your favor, the Stop Loss automatically moves upward behind the price, locking in gains and ensuring winning trades never turn into losses.
                      </p>
                    </div>
                  </div>
                </div>

                <!-- Section 6: JSON Batch Action Runner -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-code text-indigo-500"></i> 6. Programmatic Trading with Raw JSON Actions
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    For algorithmic traders, bots, and automated scripts, the platform features a dedicated action (<code>/Trading/Trading/ExecuteJson</code>) that takes a single or batched JSON payload and executes all operations in a single request:
                  </p>

                  <pre class="p-4 rounded-xl bg-slate-950 text-emerald-400 font-mono text-2xs overflow-x-auto"><code>[
                    {
                      "action": "place_order",
                      "symbol": "BTCUSDT",
                      "side": 1,           // 1 = Buy, 2 = Sell
                      "orderType": 1,      // 1 = Market, 2 = Limit, 3 = Stop
                      "quantity": 0.5,
                      "leverage": 2,
                      "stopLoss": 66000,
                      "takeProfit": 71000
                    },
                    {
                      "action": "toggle_watchlist",
                      "symbol": "AAPL",
                      "assetName": "Apple Inc."
                    },
                    {
                      "action": "reset_balance",
                      "resetBalance": 50000
                    }
                  ]</code></pre>
                </div>

                <!-- Section 7: Hangfire Price Synchronization -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-clock-rotate-left text-amber-500"></i> 7. How Hangfire Automatically Fetches Prices &amp; Triggers Stop Loss
                  </h3>
                  <p class="text-slate-600 dark:text-slate-300">
                    In real trading, you cannot stare at the screen 24 hours a day. We integrated an automated Hangfire background job (<code>ITradingPriceSyncJob</code>) that runs every single minute:
                  </p>

                  <div class="p-4 rounded-xl bg-amber-50/50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-800/40 text-xs space-y-2">
                    <p><strong>1. Real-Time Price Polling:</strong> The job fetches current market prices for every symbol currently held in open paper positions or pending limit orders.</p>
                    <p><strong>2. Protective Stop-Loss Execution:</strong> If the market drops below your Buy Stop Loss (or above a Sell Stop Loss), the position is instantly closed at that price, and the loss is subtracted from your paper cash balance.</p>
                    <p><strong>3. Profit Target Execution:</strong> If the price touches your Take Profit target, the position (or the assigned percentage) is automatically closed in green, and cash is credited.</p>
                    <p><strong>4. Pending Order Fills:</strong> When market prices hit your Limit or Stop trigger levels, the job automatically fills the order and turns it into an open position.</p>
                  </div>
                </div>

                <!-- Section 8: Balance Management & Expert Tips -->
                <div class="p-6 rounded-2xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-xs space-y-4">
                  <h3 class="text-lg font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <i class="fa-solid fa-graduation-cap text-brand-500"></i> 8. Balance Management &amp; Expert Trading Advice
                  </h3>
                  <div class="space-y-3 text-xs text-slate-600 dark:text-slate-400">
                    <p>
                      <strong>Setting &amp; Resetting Balance:</strong> Click the <strong>Set / Reset Balance</strong> button at the top of the terminal. Choose from quick presets ($10,000, $50,000, $100,000) or type any custom amount. If your strategy hits an unexpected drawdown, reset your balance and test a revised risk model with clean capital.
                    </p>
                    <div class="p-4 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 space-y-1.5">
                      <strong class="text-slate-900 dark:text-white block font-bold">Three Golden Rules for Successful Paper Trading:</strong>
                      <ol class="list-decimal pl-5 space-y-1">
                        <li><strong>Always use a 1:2 Minimum Risk-to-Reward:</strong> Never risk $100 to make $50. Aim to risk $100 only when your target profit is at least $200. This ensures profitability even with a modest 40% win rate.</li>
                        <li><strong>Never Risk More Than 1%&ndash;2% of Capital per Trade:</strong> Even the best traders face losing streaks. If you risk 10% per trade, five consecutive losses destroy 40% of your account.</li>
                        <li><strong>Treat Paper Money Like Real Savings:</strong> If you take reckless 50x trades on paper, you will build bad psychological habits that will harm your real broker account. Trade virtual money with the exact discipline you would use with your life savings.</li>
                      </ol>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            """
        };
    }
}
