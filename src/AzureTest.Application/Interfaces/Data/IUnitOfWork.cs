using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data;

public interface IUnitOfWork : IDisposable
{
    IEventRepository Events { get; }
    ITaskRepository Tasks { get; }
    IHabitTrackerRepository Habits { get; }
    IHabitLogRepository HabitLogs { get; }
    ITicketRepository Tickets { get; }
    ITicketMessageRepository TicketMessages { get; }
    IResumeRepository Resumes { get; }
    IDebtRepository Debts { get; }
    IExpenseRepository Expenses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
