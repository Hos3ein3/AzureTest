using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Media.Commands.AddItemToCustomList;
using AzureTest.Application.Media.Commands.AddWatchItem;
using AzureTest.Application.Media.Commands.ChangeWatchStatus;
using AzureTest.Application.Media.Commands.CreateCustomList;
using AzureTest.Application.Media.Commands.DeleteCustomList;
using AzureTest.Application.Media.Commands.DeleteWatchItem;
using AzureTest.Application.Media.Commands.FollowPerson;
using AzureTest.Application.Media.Commands.ImportMediaData;
using AzureTest.Application.Media.Commands.RateWatchItem;
using AzureTest.Application.Media.Commands.RemoveItemFromCustomList;
using AzureTest.Application.Media.Commands.UnfollowPerson;
using AzureTest.Application.Media.Commands.UpdateFollowedPersonNotes;
using AzureTest.Application.Media.Commands.UpdateWatchProgress;
using AzureTest.Application.Media.Commands.RewatchItem;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media;

public sealed class MediaCommandUnit : IMediaCommandUnit
{
    private readonly ICommandHandler<AddWatchItemCommand, Result<Guid>> _addWatchItem;
    private readonly ICommandHandler<UpdateWatchProgressCommand, Result> _updateProgress;
    private readonly ICommandHandler<ChangeWatchStatusCommand, Result> _changeStatus;
    private readonly ICommandHandler<RateWatchItemCommand, Result> _rateItem;
    private readonly ICommandHandler<DeleteWatchItemCommand, Result> _deleteItem;
    private readonly ICommandHandler<RewatchItemCommand, Result> _rewatchItem;
    private readonly ICommandHandler<FollowPersonCommand, Result<Guid>> _followPerson;
    private readonly ICommandHandler<UnfollowPersonCommand, Result> _unfollowPerson;
    private readonly ICommandHandler<UpdateFollowedPersonNotesCommand, Result> _updateNotes;
    private readonly ICommandHandler<CreateCustomListCommand, Result<Guid>> _createList;
    private readonly ICommandHandler<AddItemToCustomListCommand, Result> _addItemToList;
    private readonly ICommandHandler<RemoveItemFromCustomListCommand, Result> _removeItemFromList;
    private readonly ICommandHandler<DeleteCustomListCommand, Result> _deleteList;
    private readonly ICommandHandler<ImportMediaDataCommand, Result<MediaImportResultDto>> _importData;

    public MediaCommandUnit(
        ICommandHandler<AddWatchItemCommand, Result<Guid>> addWatchItem,
        ICommandHandler<UpdateWatchProgressCommand, Result> updateProgress,
        ICommandHandler<ChangeWatchStatusCommand, Result> changeStatus,
        ICommandHandler<RateWatchItemCommand, Result> rateItem,
        ICommandHandler<DeleteWatchItemCommand, Result> deleteItem,
        ICommandHandler<RewatchItemCommand, Result> rewatchItem,
        ICommandHandler<FollowPersonCommand, Result<Guid>> followPerson,
        ICommandHandler<UnfollowPersonCommand, Result> unfollowPerson,
        ICommandHandler<UpdateFollowedPersonNotesCommand, Result> updateNotes,
        ICommandHandler<CreateCustomListCommand, Result<Guid>> createList,
        ICommandHandler<AddItemToCustomListCommand, Result> addItemToList,
        ICommandHandler<RemoveItemFromCustomListCommand, Result> removeItemFromList,
        ICommandHandler<DeleteCustomListCommand, Result> deleteList,
        ICommandHandler<ImportMediaDataCommand, Result<MediaImportResultDto>> importData)
    {
        _addWatchItem = addWatchItem;
        _updateProgress = updateProgress;
        _changeStatus = changeStatus;
        _rateItem = rateItem;
        _deleteItem = deleteItem;
        _rewatchItem = rewatchItem;
        _followPerson = followPerson;
        _unfollowPerson = unfollowPerson;
        _updateNotes = updateNotes;
        _createList = createList;
        _addItemToList = addItemToList;
        _removeItemFromList = removeItemFromList;
        _deleteList = deleteList;
        _importData = importData;
    }

    public Task<Result<Guid>> AddWatchItemAsync(AddWatchItemCommand command, CancellationToken ct = default) =>
        _addWatchItem.HandleAsync(command, ct);

    public Task<Result> UpdateWatchProgressAsync(UpdateWatchProgressCommand command, CancellationToken ct = default) =>
        _updateProgress.HandleAsync(command, ct);

    public Task<Result> ChangeWatchStatusAsync(ChangeWatchStatusCommand command, CancellationToken ct = default) =>
        _changeStatus.HandleAsync(command, ct);

    public Task<Result> RateWatchItemAsync(RateWatchItemCommand command, CancellationToken ct = default) =>
        _rateItem.HandleAsync(command, ct);

    public Task<Result> DeleteWatchItemAsync(DeleteWatchItemCommand command, CancellationToken ct = default) =>
        _deleteItem.HandleAsync(command, ct);

    public Task<Result> RewatchItemAsync(RewatchItemCommand command, CancellationToken ct = default) =>
        _rewatchItem.HandleAsync(command, ct);

    public Task<Result<Guid>> FollowPersonAsync(FollowPersonCommand command, CancellationToken ct = default) =>
        _followPerson.HandleAsync(command, ct);

    public Task<Result> UnfollowPersonAsync(UnfollowPersonCommand command, CancellationToken ct = default) =>
        _unfollowPerson.HandleAsync(command, ct);

    public Task<Result> UpdateFollowedPersonNotesAsync(UpdateFollowedPersonNotesCommand command, CancellationToken ct = default) =>
        _updateNotes.HandleAsync(command, ct);

    public Task<Result<Guid>> CreateCustomListAsync(CreateCustomListCommand command, CancellationToken ct = default) =>
        _createList.HandleAsync(command, ct);

    public Task<Result> AddItemToCustomListAsync(AddItemToCustomListCommand command, CancellationToken ct = default) =>
        _addItemToList.HandleAsync(command, ct);

    public Task<Result> RemoveItemFromCustomListAsync(RemoveItemFromCustomListCommand command, CancellationToken ct = default) =>
        _removeItemFromList.HandleAsync(command, ct);

    public Task<Result> DeleteCustomListAsync(DeleteCustomListCommand command, CancellationToken ct = default) =>
        _deleteList.HandleAsync(command, ct);

    public Task<Result<MediaImportResultDto>> ImportMediaDataAsync(ImportMediaDataCommand command, CancellationToken ct = default) =>
        _importData.HandleAsync(command, ct);
}
