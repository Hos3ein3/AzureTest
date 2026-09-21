using AzureTest.Application;
using AzureTest.Infrastructure;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Persistence;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// Read Infrastructure Feature Options
var infraOptions = new InfrastructureFeatureOptions();
builder.Configuration.GetSection("FeatureManagement").Bind(infraOptions);

// Add layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(infraOptions, builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

// Feature Management
builder.Services.AddFeatureManagement();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Add HealthChecks UI with In-Memory Storage
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecksUI(options => 
{
    options.UIPath = "/healthchecks-ui";
});
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();