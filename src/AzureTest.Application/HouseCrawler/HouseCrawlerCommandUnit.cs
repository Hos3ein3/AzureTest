using AzureTest.Application.HouseCrawler.Commands;
using AzureTest.Application.HouseCrawler.Interfaces;
using AzureTest.Application.Messaging;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.HouseCrawler;

public class HouseCrawlerCommandUnit : IHouseCrawlerCommandUnit
{
    public ICommandHandler<CreateHouseSearchHistoryCommand, Result> CreateHistory { get; }
    public ICommandHandler<SaveHouseCredentialCommand, Result> SaveCredential { get; }

    public HouseCrawlerCommandUnit(
        ICommandHandler<CreateHouseSearchHistoryCommand, Result> createHistory,
        ICommandHandler<SaveHouseCredentialCommand, Result> saveCredential)
    {
        CreateHistory = createHistory;
        SaveCredential = saveCredential;
    }
}
