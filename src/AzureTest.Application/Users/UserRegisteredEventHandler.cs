using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Users;

public record UserRegisteredEvent(Guid UserId, string Email);

public class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;

    public UserRegisteredEventHandler(INoSqlUnitOfWork noSqlUnitOfWork)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
    }

    public async Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        var existing = await _noSqlUnitOfWork.UserPreferences.GetByUserIdAsync(@event.UserId, cancellationToken);
        if (existing == null)
        {
            var pref = new UserPreference
            {
                Id = Guid.CreateVersion7(),
                UserId = @event.UserId,
                ThemeColor = "blue",
                Language = "en",
                Currency = "USD",
                PreferredDashboard = ""
            };
            await _noSqlUnitOfWork.UserPreferences.AddAsync(pref, cancellationToken);
        }
    }
}
