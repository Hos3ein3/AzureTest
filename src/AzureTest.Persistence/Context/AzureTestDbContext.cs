

using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureTest.Persistence.Context;

public class AzureTestDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly DomainEventsInterceptor? _domainEventsInterceptor;

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options,
        DomainEventsInterceptor? domainEventsInterceptor = null) : base(options)
    {
        _domainEventsInterceptor = domainEventsInterceptor;
    }

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options) : base(options)
    {
    }

    protected AzureTestDbContext()
    {
    }

    /// <summary>Exposes collected domain events after SaveChanges for dispatch.</summary>
    public DomainEventsInterceptor? DomainEventsInterceptor => _domainEventsInterceptor;

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // New Feature Domains
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<HabitTracker> HabitTrackers => Set<HabitTracker>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<Debt> Debts => Set<Debt>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Identity Schema Mapping
        modelBuilder.Entity<ApplicationUser>().ToTable("Users", "Identity");
        modelBuilder.Entity<ApplicationRole>().ToTable("Roles", "Identity");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "Identity");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "Identity");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "Identity");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "Identity");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "Identity");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens", "Identity");

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

