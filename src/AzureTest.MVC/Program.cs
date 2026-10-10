using WebMarkupMin.AspNetCore8;
using WebMarkupMin.Core;
using Microsoft.AspNetCore.DataProtection;



using AzureTest.Application;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using AzureTest.Persistence;
using AzureTest.Persistence.Context;

using Hangfire;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;


var builder = WebApplication.CreateBuilder(args);

// Read Infrastructure Feature Options
var infraOptions = new InfrastructureFeatureOptions();
builder.Configuration.GetSection("FeatureManagement").Bind(infraOptions);

// Add layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(infraOptions, builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// Identity configuration
var identitySettings = new AzureTest.Infrastructure.Configuration.IdentitySettings();
builder.Configuration.GetSection(AzureTest.Infrastructure.Configuration.IdentitySettings.SectionName).Bind(identitySettings);

builder.Services.AddIdentity<AzureTest.Domain.Entities.Identity.ApplicationUser, AzureTest.Domain.Entities.Identity.ApplicationRole>(options =>
{
    options.Password.RequireDigit = identitySettings.Password.RequireDigit;
    options.Password.RequiredLength = identitySettings.Password.RequiredLength;
    options.Password.RequireNonAlphanumeric = identitySettings.Password.RequireNonAlphanumeric;
    options.Password.RequireUppercase = identitySettings.Password.RequireUppercase;
    options.Password.RequireLowercase = identitySettings.Password.RequireLowercase;
    options.Password.RequiredUniqueChars = identitySettings.Password.RequiredUniqueChars;

    options.Lockout.AllowedForNewUsers = identitySettings.Lockout.AllowedForNewUsers;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.Lockout.DefaultLockoutTimeSpanMinutes);
    options.Lockout.MaxFailedAccessAttempts = identitySettings.Lockout.MaxFailedAccessAttempts;

    options.User.AllowedUserNameCharacters = identitySettings.User.AllowedUserNameCharacters;
    options.User.RequireUniqueEmail = identitySettings.User.RequireUniqueEmail;

    options.SignIn.RequireConfirmedEmail = identitySettings.SignIn.RequireConfirmedEmail;
    options.SignIn.RequireConfirmedPhoneNumber = identitySettings.SignIn.RequireConfirmedPhoneNumber;
})
    .AddEntityFrameworkStores<AzureTest.Persistence.Context.AzureTestDbContext>()
    .AddClaimsPrincipalFactory<AzureTest.Infrastructure.Services.CustomUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

if (infraOptions.UseJWT)
{
    // JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "fallback-super-secret-key-1234567890"))
        };
        // Support getting token from cookie for MVC views
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt")) { context.Token = context.Request.Cookies["jwt"]; }
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Auth/Login");
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });
}
else
{
    // Normal Identity Cookie Authentication
    builder.Services.Configure<Microsoft.AspNetCore.Identity.SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.FromHours(12);
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });
}

// Feature Management
builder.Services.AddFeatureManagement();

// Add services to the container.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

builder.Services.AddControllersWithViews(options => 
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
    options.Filters.Add(typeof(AzureTest.MVC.Filters.DynamicAuthorizationFilter));
})
    .AddRazorRuntimeCompilation();


builder.Services.AddScoped<AzureTest.MVC.Services.IDashboardDataLoader, AzureTest.MVC.Services.DashboardDataLoader>();
builder.Services.AddScoped<AzureTest.Application.Interfaces.Services.IPermissionScanner, AzureTest.MVC.Services.PermissionScanner>();
builder.Services.AddScoped<AzureTest.MVC.Filters.DynamicAuthorizationFilter>();

// Add HealthChecks UI with In-Memory Storage
var hcSettings = builder.Configuration.GetSection("HealthChecksUI").Get<HealthCheckUISettings>();
builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("AzureTest Health", "/health");
    if (hcSettings != null)
    {
        setup.SetEvaluationTimeInSeconds(hcSettings.EvaluationTimeInSeconds);
        setup.SetMinimumSecondsBetweenFailureNotifications(hcSettings.MinimumSecondsBetweenFailureNotifications);
    }
}).AddInMemoryStorage();

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates.
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddExceptionHandler<AzureTest.MVC.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

// --- HTML, CSS, JS Minification (Only enabled in Production) ---
if (builder.Environment.IsProduction())
{
    // WebOptimizer for CSS & JS minification
    builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.MinifyJsFiles("js/**/*.js");
        pipeline.MinifyCssFiles("css/**/*.css");
    });

    // WebMarkupMin for HTML minification
    builder.Services.AddWebMarkupMin(options =>
    {
        options.AllowMinificationInDevelopmentEnvironment = false;
        options.AllowCompressionInDevelopmentEnvironment = false;
    })
    .AddHtmlMinification(options =>
    {
        options.MinificationSettings.RemoveRedundantAttributes = true;
        options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
        options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
        options.MinificationSettings.RemoveHtmlComments = true;
    })
    .AddHttpCompression();
}


// ── ProductionSimulate: start Testcontainers & override connection strings ────
if (builder.Environment.IsEnvironment("ProductionSimulate"))
    await AzureTest.MVC.Infrastructure.ProductionSimulateSetup.ConfigureAsync(builder);
// ── ProductionSimulateLocal: no container bootstrap needed — reads from appsettings.ProductionSimulateLocal.json ──

// FIX: Rename mismatched widget partial views
try 
{
    var widgetsDir = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Views", "Shared", "Widgets");
    var renames = new System.Collections.Generic.Dictionary<string, string>
    {
        { "_WidgetApplicationsByStatusChart.cshtml", "_WidgetApplicationByStatus.cshtml" },
        { "_WidgetApplicationsOverTimeChart.cshtml", "_WidgetApplicationOverTime.cshtml" },
        { "_WidgetApplicationSourcesChart.cshtml", "_WidgetApplicationSources.cshtml" },
        { "_WidgetDebtsByPersonChart.cshtml", "_WidgetDebtsByPerson.cshtml" },
        { "_WidgetExpensesByCategoryChart.cshtml", "_WidgetExpensesByCategory.cshtml" },
        { "_WidgetHabitAdherenceChart.cshtml", "_WidgetHabitAdherence.cshtml" },
        { "_WidgetMonthlyFinanceChart.cshtml", "_WidgetMonthlyCashFlow.cshtml" },
        { "_WidgetTicketStatusChart.cshtml", "_WidgetSupportTickets.cshtml" },
        { "_WidgetTopLocationsChart.cshtml", "_WidgetTopLocations.cshtml" },
        { "_WidgetTopPositionsChart.cshtml", "_WidgetTopPositions.cshtml" }
    };
    foreach(var r in renames)
    {
        var oldPath = System.IO.Path.Combine(widgetsDir, r.Key);
        var newPath = System.IO.Path.Combine(widgetsDir, r.Value);
        if (System.IO.File.Exists(oldPath)) System.IO.File.Move(oldPath, newPath, true);
    }
} 
catch (Exception ex) 
{
    Console.WriteLine("Failed to rename widgets: " + ex.Message);
}

var app = builder.Build();

// ── Auto-apply pending EF Core migrations on cold start ─────────────────────
var isSimulateEnv = app.Environment.IsEnvironment("ProductionSimulate") ||
                    app.Environment.IsEnvironment("ProductionSimulateLocal");
if (!isSimulateEnv)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AzureTestDbContext>();
        var pending = await db.Database.GetPendingMigrationsAsync();
        if (pending.Any())
        {
            app.Logger.LogInformation("Applying {Count} pending migration(s)...", pending.Count());
            await db.Database.MigrateAsync();
            app.Logger.LogInformation("Migrations applied successfully.");
        }

        // ── Auto-seed IND Companies on cold start ────────────────────────────────
        var companySeeder = scope.ServiceProvider.GetService<AzureTest.Persistence.Seeding.CompanyDataSeeder>();
        if (companySeeder != null)
        {
            await companySeeder.SeedIfEmptyAsync();
        }

        var identitySeeder = scope.ServiceProvider.GetService<AzureTest.Infrastructure.Seeding.IdentityDataSeeder>();
        if (identitySeeder != null)
            await identitySeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying migrations.");
        throw;
    }
}

// ── ProductionSimulate: apply Testcontainer migrations + seed all Bogus data ───
 if (app.Environment.IsEnvironment("ProductionSimulate"))
{
    try
    {
        await AzureTest.MVC.Infrastructure.ProductionSimulateSetup.SeedAsync(app);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred during ProductionSimulate seeding.");
        throw;
    }
}

// ── ProductionSimulateLocal: migrate + seed if empty (uses real local services) ──
if (app.Environment.IsEnvironment("ProductionSimulateLocal"))
{
    try
    {
        await AzureTest.MVC.Infrastructure.ProductionSimulateLocalSetup.SeedIfEmptyAsync(app);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred during ProductionSimulateLocal seeding.");
        throw;
    }
}

// FIX: Deduplicate Components
try
{
    using var scope = app.Services.CreateScope();
    var noSql = scope.ServiceProvider.GetRequiredService<AzureTest.Application.Interfaces.Data.NoSql.INoSqlUnitOfWork>();
    var components = await noSql.DashboardComponents.GetAllAsync();
    var duplicates = components.GroupBy(c => c.Name).Where(g => g.Count() > 1).ToList();
    foreach (var group in duplicates)
    {
        var toDelete = group.Skip(1).ToList();
        foreach (var item in toDelete)
        {
            await noSql.DashboardComponents.RemoveAsync(item.Id);
        }
    }
    if (duplicates.Any())
    {
        await noSql.SaveChangesAsync();
        app.Logger.LogInformation("Removed duplicate dashboard components.");
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to deduplicate components.");
}

app.Logger.LogInformation(app.Environment.EnvironmentName, "Environment {0}");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseWebOptimizer();
    app.UseWebMarkupMin();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<AzureTest.MVC.Hubs.NotificationHub>("/notificationHub");
app.MapHub<AzureTest.MVC.Hubs.FantasyAuctionHub>("/auctionHub");

app.MapControllerRoute(
        name: "dashboard_dynamic",
        pattern: "{slug}",
        defaults: new { controller = "Dashboard", action = "ViewDashboard" },
        constraints: new { slug = new AzureTest.MVC.Infrastructure.DashboardRouteConstraint() }
    ).WithStaticAssets();


app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHealthCheck"))
{
    app.MapHealthChecksUI(options => 
    {
        options.UIPath = "/healthchecks-ui";
    }).AllowAnonymous();
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();
}
app.MapPrometheusScrapingEndpoint().AllowAnonymous();
if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHangfire"))
{
    app.UseHangfireDashboard("/hangfire");
}

app.Run();