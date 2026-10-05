using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.HouseCrawler.Commands;

public record CreateHouseSearchHistoryCommand(Guid UserId, string City, string MinPrice, string MaxPrice, string Bedrooms, string Source, bool EmailAlertsEnabled) : ICommand<Result>;

public class CreateHouseSearchHistoryCommandHandler : ICommandHandler<CreateHouseSearchHistoryCommand, Result>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ILogger<CreateHouseSearchHistoryCommandHandler> _logger;

    public CreateHouseSearchHistoryCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, ILogger<CreateHouseSearchHistoryCommandHandler> logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CreateHouseSearchHistoryCommand command, CancellationToken cancellationToken = default)
    {
        var history = new HouseSearchHistory
        {
            UserId = command.UserId,
            City = command.City,
            MinPrice = command.MinPrice,
            MaxPrice = command.MaxPrice,
            Bedrooms = command.Bedrooms,
            Source = command.Source,
            EmailAlertsEnabled = command.EmailAlertsEnabled
        };

        await _noSqlUnitOfWork.HouseSearchHistories.AddAsync(history);
        return Result.Success();
    }
}
