using AzureTest.Domain.Entities;
using AzureTest.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AzureTest.Persistence.Context;

public class AzureTestDbContext : DbContext
{
    private readonly DomainEventsInterceptor _domainEventsInterceptor;

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options,
        DomainEventsInterceptor domainEventsInterceptor) : base(options)
    {
        _domainEventsInterceptor = domainEventsInterceptor;
    }

    public DbSet<Resume> Resumes => Set<Resume>();

    /// <summary>Exposes collected domain events after SaveChanges for dispatch.</summary>
    public DomainEventsInterceptor DomainEventsInterceptor => _domainEventsInterceptor;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

