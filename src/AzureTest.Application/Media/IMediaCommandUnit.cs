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
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media;

public interface IMediaCommandUnit
{
    Task<Result<Guid>> AddWatchItemAsync(AddWatchItemCommand command, CancellationToken ct = default);
    Task<Result> UpdateWatchProgressAsync(UpdateWatchProgressCommand command, CancellationToken ct = default);
    Task<Result> ChangeWatchStatusAsync(ChangeWatchStatusCommand command, CancellationToken ct = default);
    Task<Result> RateWatchItemAsync(RateWatchItemCommand command, CancellationToken ct = default);
    Task<Result> DeleteWatchItemAsync(DeleteWatchItemCommand command, CancellationToken ct = default);
    Task<Result> RewatchItemAsync(RewatchItemCommand command, CancellationToken ct = default);
    Task<Result<Guid>> FollowPersonAsync(FollowPersonCommand command, CancellationToken ct = default);
    Task<Result> UnfollowPersonAsync(UnfollowPersonCommand command, CancellationToken ct = default);
    Task<Result> UpdateFollowedPersonNotesAsync(UpdateFollowedPersonNotesCommand command, CancellationToken ct = default);
    Task<Result<Guid>> CreateCustomListAsync(CreateCustomListCommand command, CancellationToken ct = default);
    Task<Result> AddItemToCustomListAsync(AddItemToCustomListCommand command, CancellationToken ct = default);
    Task<Result> RemoveItemFromCustomListAsync(RemoveItemFromCustomListCommand command, CancellationToken ct = default);
    Task<Result> DeleteCustomListAsync(DeleteCustomListCommand command, CancellationToken ct = default);
    Task<Result<MediaImportResultDto>> ImportMediaDataAsync(ImportMediaDataCommand command, CancellationToken ct = default);
}
