using AzureTest.Application;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Persistence;
using AzureTest.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Hangfire;

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
builder.Services.AddControllersWithViews(options => 
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
})
    .AddRazorRuntimeCompilation();

// Add HealthChecks UI with In-Memory Storage
builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("AzureTest Health", "/health");
})
.AddInMemoryStorage();

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates.
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddExceptionHandler<AzureTest.MVC.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ── Auto-apply pending EF Core migrations on cold start ─────────────────────
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
    {
        await identitySeeder.SeedAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while applying migrations.");
    throw;
}

app.Logger.LogInformation(app.Environment.EnvironmentName, "Environment {0}");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecksUI(options => 
{
    options.UIPath = "/healthchecks-ui";
}).AllowAnonymous();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.MapPrometheusScrapingEndpoint().AllowAnonymous();
if (builder.Configuration.GetValue<bool>("FeatureManagement:EnableHangfire"))
{
    app.UseHangfireDashboard("/hangfire");
}

app.Run();