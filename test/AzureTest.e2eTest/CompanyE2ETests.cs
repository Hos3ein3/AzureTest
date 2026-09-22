using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace AzureTest.e2eTest;

[CollectionDefinition("E2E Tests", DisableParallelization = true)]
public class E2ETestCollection : ICollectionFixture<E2ETestWebAppFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

[Collection("E2E Tests")]
public class CompanyE2ETests
{
    private readonly E2ETestWebAppFactory _factory;

    public CompanyE2ETests(E2ETestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_NavigateToCompanies_And_CreateNewCompany()
    {
        // Arrange
        using var playwright = await Playwright.CreateAsync();
        
        // Use Chromium. In CI, we usually run headless. We can run headless locally too.
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
        { 
            Headless = true 
        });
        
        var page = await browser.NewPageAsync();

        // Act & Assert

        // 1. Navigate to the root URL (which redirects to or has a link to Companies, or just go to /Company)
        await page.GotoAsync($"{_factory.ServerAddress}/Company");

        // Verify we are on the Companies page
        var header = page.Locator("h1:has-text('Companies')");
        await Expect(header).ToBeVisibleAsync();

        // 2. Click "Add New"
        await page.ClickAsync("a:has-text('Add New')");

        // Verify we navigated to Create page
        var createHeader = page.Locator("h5:has-text('Add New Company')");
        await Expect(createHeader).ToBeVisibleAsync();

        // 3. Fill out the form
        string testCompanyName = "E2E Test Company " + System.Guid.NewGuid().ToString().Substring(0, 8);
        await page.FillAsync("input[name='CompanyName']", testCompanyName);
        await page.FillAsync("input[name='Location']", "E2E City");
        await page.FillAsync("input[name='KvkNumber']", "88888888");
        await page.CheckAsync("input[name='IsINDMember']");

        // 4. Submit the form
        await page.ClickAsync("button:has-text('Save')");

        // 5. Verify redirect back to Index and success message
        var successAlert = page.Locator("div[role='alert']:has-text('Company created successfully')");
        // We might not know the exact success text, but we know it should redirect back to /Company
        
        // Wait for the URL to be back at /Company
        await page.WaitForURLAsync($"{_factory.ServerAddress}/Company");

        // 6. Verify the new company is in the table
        // We can search for it using the filter
        await page.FillAsync("input[name='Name']", testCompanyName);
        await page.ClickAsync("button:has-text('Apply')");

        // The table should contain the new company
        var cell = page.Locator($"td:has-text('{testCompanyName}')");
        await Expect(cell).ToBeVisibleAsync();
    }

    // Custom helper for Playwright Expect
    private ILocatorAssertions Expect(ILocator locator) => Microsoft.Playwright.Assertions.Expect(locator);
}
