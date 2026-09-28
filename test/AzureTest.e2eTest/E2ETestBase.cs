using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace AzureTest.e2eTest;

/// <summary>
/// Reusable base class for all E2E Playwright tests.
///
/// Usage — simply inherit this in your test class:
///
///   [Collection("E2E Tests")]
///   public class MyFeatureE2ETests : E2ETestBase
///   {
///       public MyFeatureE2ETests(E2ETestWebAppFactory factory) : base(factory) { }
///
///       [Fact]
///       public async Task My_Test()
///       {
///           var (ctx, page) = await CreateAuthenticatedPageAsync();
///           await using var _ = ctx;
///           // ... your assertions
///       }
///   }
///
/// The factory starts real Postgres + MongoDB Testcontainers so every test
/// class shares one expensive boot cycle (via xUnit's [Collection] fixture).
/// </summary>
public abstract class E2ETestBase
{
    // ──────────────────────────────────────────────────────────────────────────
    // Configuration constants  (override in derived classes if needed)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Default admin credentials seeded by IdentityDataSeeder.</summary>
    protected virtual string DefaultAdminEmail    => "admin@azuretest.local";
    protected virtual string DefaultAdminPassword => "123456";

    /// <summary>Default regular-user credentials seeded by IdentityDataSeeder.</summary>
    protected virtual string DefaultUserEmail    => "user@azuretest.com";
    protected virtual string DefaultUserPassword => "User@123";

    // ──────────────────────────────────────────────────────────────────────────
    // Fields
    // ──────────────────────────────────────────────────────────────────────────

    protected readonly E2ETestWebAppFactory Factory;

    /// <summary>Base URL of the Kestrel server started by the factory.</summary>
    protected string BaseUrl => Factory.ServerAddress;

    // ──────────────────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────────────────

    protected E2ETestBase(E2ETestWebAppFactory factory)
    {
        Factory = factory;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Browser helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a fresh Chromium browser context for a single test.
    /// Dispose the returned context at the end of the test (use <c>await using</c>).
    /// </summary>
    /// <param name="headless">
    ///   Run headless (default true).
    ///   Set to <c>false</c> locally when you want to watch the browser.
    /// </param>
    protected async Task<IBrowserContext> CreateBrowserContextAsync(bool headless = true)
    {
        var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless,
            SlowMo   = headless ? 0 : 100   // slow-motion when watching
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            // Accept any self-signed certificate the test Kestrel might expose
            IgnoreHTTPSErrors = true,
            // 1920x1080 gives consistent screenshots / element positions
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        // Give every navigation ample time on a cold container
        context.SetDefaultNavigationTimeout(30_000);
        context.SetDefaultTimeout(15_000);

        return context;
    }

    /// <summary>
    /// Navigates to the login page and signs in with the supplied credentials.
    /// After this method returns, <paramref name="page"/> is on the home page
    /// and the auth cookie is set in the browser context.
    /// </summary>
    protected async Task LoginAsync(IPage page, string? email = null, string? password = null)
    {
        email    ??= DefaultAdminEmail;
        password ??= DefaultAdminPassword;

        await page.GotoAsync($"{BaseUrl}/Auth/Login");

        // Wait for the login form to be visible
        await page.WaitForSelectorAsync("input[name='Email']");

        await page.FillAsync("input[name='Email']",    email);
        await page.FillAsync("input[name='Password']", password);
        await page.ClickAsync("button[type='submit']");

        // Wait until we've left the login page
        await page.WaitForURLAsync(url => !url.Contains("/Auth/Login") && !url.Contains("/Account/Login"));
    }

    /// <summary>
    /// Creates a browser context, opens a page, logs in, and returns both.
    /// The caller is responsible for disposing the context.
    /// </summary>
    protected async Task<(IBrowserContext Context, IPage Page)> CreateAuthenticatedPageAsync(
        string? email    = null,
        string? password = null,
        bool headless    = true)
    {
        var context = await CreateBrowserContextAsync(headless);
        var page    = await context.NewPageAsync();
        await LoginAsync(page, email, password);
        return (context, page);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Assertion helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Shorthand for Playwright Expect on a locator.</summary>
    protected static ILocatorAssertions Expect(ILocator locator)
        => Microsoft.Playwright.Assertions.Expect(locator);

    /// <summary>Shorthand for Playwright Expect on a page.</summary>
    protected static IPageAssertions Expect(IPage page)
        => Microsoft.Playwright.Assertions.Expect(page);

    // ──────────────────────────────────────────────────────────────────────────
    // Navigation helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Builds an absolute URL from a relative path.</summary>
    protected string Url(string relativePath)
        => $"{BaseUrl}/{relativePath.TrimStart('/')}"; 

    /// <summary>Navigates and waits for DOM content to be loaded.</summary>
    protected async Task GoToAsync(IPage page, string relativePath)
        => await page.GotoAsync(Url(relativePath), new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
}
