using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Playwright;
using Shouldly;
using Xunit;

namespace AzureTest.e2eTest;

/// <summary>
/// E2E tests for the Habit Tracker feature.
/// Covers: Create a new habit via the UI.
/// </summary>
[Collection("E2E Tests")]
public class HabitE2ETests : E2ETestBase
{
    private readonly IFixture _fixture = new Fixture();

    public HabitE2ETests(E2ETestWebAppFactory factory) : base(factory) { }

    // ──────────────────────────────────────────────────────────────────────────
    // CREATE tests
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_BooleanHabit_ShouldAppearInHabitList()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        var (ctx, page) = await CreateAuthenticatedPageAsync();
        await using var _ = ctx;

        var habitName = $"E2E Boolean Habit {_fixture.Create<string>().Substring(0, 8)}";
        const string description = "Created by E2E test";
        const string habitType   = "Boolean";     // HabitType enum value
        const string frequency   = "Daily";        // HabitFrequency enum value
        const double goal        = 1.0;

        // ── Act ───────────────────────────────────────────────────────────────
        // Navigate to the Habit index page
        await GoToAsync(page, "/Habit");

        // Verify we are on the Habits page
        await Expect(page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Habit", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        // Open the create habit modal or form
        // The index page is expected to have a button/link that opens or reveals the create form
        var openCreateFormButton = page.Locator("[data-habit='create-btn'], button:has-text('Add Habit'), a:has-text('Add Habit'), button:has-text('New Habit'), a:has-text('New Habit')");
        await openCreateFormButton.First.ClickAsync();

        // Fill in the habit name
        var nameInput = page.Locator("input[name='name'], input[name='Name'], #habitName");
        await nameInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nameInput.First.FillAsync(habitName);

        // Fill in the description
        var descriptionInput = page.Locator("input[name='description'], textarea[name='description'], input[name='Description'], textarea[name='Description'], #habitDescription");
        if (await descriptionInput.CountAsync() > 0)
        {
            await descriptionInput.First.FillAsync(description);
        }

        // Select HabitType = Boolean
        var typeSelect = page.Locator("select[name='type'], select[name='Type'], #habitType");
        await typeSelect.First.SelectOptionAsync(new SelectOptionValue { Label = habitType });

        // Select HabitFrequency = Daily
        var frequencySelect = page.Locator("select[name='frequency'], select[name='Frequency'], #habitFrequency");
        await frequencySelect.First.SelectOptionAsync(new SelectOptionValue { Label = frequency });

        // Set Goal
        var goalInput = page.Locator("input[name='goal'], input[name='Goal'], #habitGoal");
        if (await goalInput.CountAsync() > 0)
        {
            await goalInput.First.FillAsync(goal.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        // Submit the form
        var submitButton = page.Locator("button[type='submit']:has-text('Save'), button[type='submit']:has-text('Create'), button[type='submit']:has-text('Add')");
        await submitButton.First.ClickAsync();

        // ── Assert ────────────────────────────────────────────────────────────
        // After form submission, we should be redirected back to the Habit index
        await page.WaitForURLAsync(url => url.Contains("/Habit"), new PageWaitForURLOptions { Timeout = 10_000 });

        // The new habit should be visible in the list
        var habitEntry = page.Locator($"text={habitName}");
        await Expect(habitEntry.First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Create_NumericHabit_WithGoal_ShouldAppearInHabitList()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        var (ctx, page) = await CreateAuthenticatedPageAsync();
        await using var _ = ctx;

        var habitName = $"E2E Numeric Habit {_fixture.Create<string>().Substring(0, 8)}";
        const string habitType = "Numeric";
        const string frequency = "Weekly";
        const double goal      = 5.0;

        // ── Act ───────────────────────────────────────────────────────────────
        await GoToAsync(page, "/Habit");

        var openCreateFormButton = page.Locator("[data-habit='create-btn'], button:has-text('Add Habit'), a:has-text('Add Habit'), button:has-text('New Habit'), a:has-text('New Habit')");
        await openCreateFormButton.First.ClickAsync();

        var nameInput = page.Locator("input[name='name'], input[name='Name'], #habitName");
        await nameInput.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await nameInput.First.FillAsync(habitName);

        var typeSelect = page.Locator("select[name='type'], select[name='Type'], #habitType");
        await typeSelect.First.SelectOptionAsync(new SelectOptionValue { Label = habitType });

        var frequencySelect = page.Locator("select[name='frequency'], select[name='Frequency'], #habitFrequency");
        await frequencySelect.First.SelectOptionAsync(new SelectOptionValue { Label = frequency });

        var goalInput = page.Locator("input[name='goal'], input[name='Goal'], #habitGoal");
        if (await goalInput.CountAsync() > 0)
        {
            await goalInput.First.FillAsync(goal.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        var submitButton = page.Locator("button[type='submit']:has-text('Save'), button[type='submit']:has-text('Create'), button[type='submit']:has-text('Add')");
        await submitButton.First.ClickAsync();

        // ── Assert ────────────────────────────────────────────────────────────
        await page.WaitForURLAsync(url => url.Contains("/Habit"), new PageWaitForURLOptions { Timeout = 10_000 });

        var habitEntry = page.Locator($"text={habitName}");
        await Expect(habitEntry.First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Create_Habit_RequiresAuthentication_RedirectsToLogin()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        // Use a fresh context WITHOUT logging in
        var ctx  = await CreateBrowserContextAsync();
        await using var _ = ctx;
        var page = await ctx.NewPageAsync();

        // ── Act ───────────────────────────────────────────────────────────────
        await GoToAsync(page, "/Habit");

        // ── Assert ────────────────────────────────────────────────────────────
        // Should redirect to the login page because [Authorize] is on HabitController
        var currentUrl = page.Url;
        currentUrl.ShouldContain("Login",
            customMessage: $"Expected redirect to Login but got: {currentUrl}");
    }
}
