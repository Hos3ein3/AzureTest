

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Context;

namespace AzureTest.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AzureTestDbContext _dbContext;

    private IEventRepository? _events;
    private ITaskRepository? _tasks;
    private IHabitTrackerRepository? _habits;
    private IHabitLogRepository? _habitLogs;
    private ITicketRepository? _tickets;
    private ITicketMessageRepository? _ticketMessages;
    private IResumeRepository? _resumes;
    private IDebtRepository? _debts;
    private IExpenseRepository? _expenses;

    private IPermissionRepository? _permissions;
    private IRolePermissionRepository? _rolePermissions;

    public UnitOfWork(AzureTestDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IEventRepository Events => _events ??= new EventRepository(_dbContext);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_dbContext);
    public IHabitTrackerRepository Habits => _habits ??= new HabitTrackerRepository(_dbContext);
    public IHabitLogRepository HabitLogs => _habitLogs ??= new HabitLogRepository(_dbContext);
    public ITicketRepository Tickets => _tickets ??= new TicketRepository(_dbContext);
    public ITicketMessageRepository TicketMessages => _ticketMessages ??= new TicketMessageRepository(_dbContext);
    public IResumeRepository Resumes => _resumes ??= new ResumeRepository(_dbContext);
    public IDebtRepository Debts => _debts ??= new DebtRepository(_dbContext);
    public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_dbContext);
    public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_dbContext);
    public IRolePermissionRepository RolePermissions => _rolePermissions ??= new RolePermissionRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns domain events collected by the DomainEventsInterceptor after the last SaveChanges.
    /// Call this after SaveChangesAsync to dispatch events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        return _dbContext.DomainEventsInterceptor.PopDomainEvents();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}

