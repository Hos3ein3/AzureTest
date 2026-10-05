using AzureTest.Application.Messaging;
using AzureTest.Application.Abstractions;
using AzureTest.Application.HouseCrawler.Commands;

namespace AzureTest.Application.HouseCrawler.Interfaces;

public interface IHouseCrawlerCommandUnit
{
    ICommandHandler<CreateHouseSearchHistoryCommand, Result> CreateHistory { get; }
    ICommandHandler<SaveHouseCredentialCommand, Result> SaveCredential { get; }
}
