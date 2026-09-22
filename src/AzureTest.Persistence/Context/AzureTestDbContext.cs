using AzureTest.Domain.Entities;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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

        // Apply a global query filter for all ISoftDelete entities.
        // Automatically excludes soft-deleted records from every query.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            var param = Expression.Parameter(entityType.ClrType, "e");
            var prop = Expression.Property(param, nameof(ISoftDelete.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}

