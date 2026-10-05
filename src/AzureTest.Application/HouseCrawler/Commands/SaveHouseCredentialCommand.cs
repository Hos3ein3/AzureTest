using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.HouseCrawler.Commands;

public record SaveHouseCredentialCommand(Guid UserId, HouseWebsite Website, string Username, string Password) : ICommand<Result>;

public class SaveHouseCredentialCommandHandler : ICommandHandler<SaveHouseCredentialCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ILogger<SaveHouseCredentialCommandHandler> _logger;

    public SaveHouseCredentialCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, ILogger<SaveHouseCredentialCommandHandler> logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(SaveHouseCredentialCommand command, CancellationToken cancellationToken = default)
    {
        var existingCredentials = await _noSqlUnitOfWork.HouseCrawlerCredentials.GetAllAsync(cancellationToken);
        var cred = existingCredentials.FirstOrDefault(c => c.UserId == command.UserId && c.Website == command.Website);

        if (cred != null)
        {
            cred.Username = command.Username;
            cred.PasswordHash = command.Password; // simple text for now
            cred.LastUsedOn = DateTime.UtcNow;
            await _noSqlUnitOfWork.HouseCrawlerCredentials.UpdateAsync(cred.Id, cred, cancellationToken);
        }
        else
        {
            cred = new HouseCrawlerCredential
            {
                UserId = command.UserId,
                Website = command.Website,
                Username = command.Username,
                PasswordHash = command.Password
            };
            await _noSqlUnitOfWork.HouseCrawlerCredentials.AddAsync(cred, cancellationToken);
        }

        return Result.Success();
    }
}
