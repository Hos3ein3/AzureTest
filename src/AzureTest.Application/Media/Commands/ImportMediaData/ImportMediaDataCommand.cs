using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Documents.Media;
using FluentValidation;

namespace AzureTest.Application.Media.Commands.ImportMediaData;

public sealed record ImportMediaDataCommand(
    Guid UserId,
    Guid TenantId,
    string JsonData) : ICommand<Result<MediaImportResultDto>>;

public sealed class ImportMediaDataCommandValidator : AbstractValidator<ImportMediaDataCommand>
{
    public ImportMediaDataCommandValidator()
    {
        RuleFor(x => x.JsonData).NotEmpty().WithMessage("JSON payload is required.");
    }
}

public sealed class ImportMediaDataCommandHandler : ICommandHandler<ImportMediaDataCommand, Result<MediaImportResultDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public ImportMediaDataCommandHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MediaImportResultDto>> HandleAsync(ImportMediaDataCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var exportData = JsonSerializer.Deserialize<MediaExportDto>(cmd.JsonData, options);

            if (exportData == null)
            {
                return Result.Failure<MediaImportResultDto>("Invalid JSON format.");
            }

            int watchItemsCount = 0;
            int peopleCount = 0;
            int listsCount = 0;
            var errors = new List<string>();

            // Import watch items
            if (exportData.WatchItems != null)
            {
                foreach (var item in exportData.WatchItems)
                {
                    try
                    {
                        var existing = await _uow.WatchItems.GetByUserAndTmdbAsync(cmd.UserId, item.TmdbId, item.Type, ct);
                        if (existing == null)
                        {
                            var doc = WatchItemDocument.Create(
                                cmd.UserId,
                                cmd.TenantId,
                                item.TmdbId,
                                item.Type,
                                item.Title,
                                item.OriginalTitle,
                                item.PosterPath,
                                item.BackdropPath,
                                item.Overview,
                                item.ReleaseDate,
                                item.TmdbVoteAverage,
                                item.Status,
                                item.CurrentSeason,
                                item.CurrentEpisode,
                                item.TotalSeasons,
                                item.TotalEpisodes,
                                item.RuntimeMinutes,
                                item.Genres
                            );

                            if (item.UserRating.HasValue || !string.IsNullOrWhiteSpace(item.UserReview))
                            {
                                doc.RateAndReview(item.UserRating, item.UserReview, item.WatchedDate);
                            }

                            await _uow.WatchItems.AddAsync(doc, ct);
                            watchItemsCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to import watch item '{item.Title}': {ex.Message}");
                    }
                }
            }

            // Import followed people
            if (exportData.FollowedPeople != null)
            {
                foreach (var person in exportData.FollowedPeople)
                {
                    try
                    {
                        var existing = await _uow.FollowedPeople.GetByUserAndPersonAsync(cmd.UserId, person.TmdbPersonId, ct);
                        if (existing == null)
                        {
                            var doc = FollowedPersonDocument.Create(
                                cmd.UserId,
                                cmd.TenantId,
                                person.TmdbPersonId,
                                person.Name,
                                person.KnownForDepartment,
                                person.ProfilePath,
                                person.Notes
                            );
                            await _uow.FollowedPeople.AddAsync(doc, ct);
                            peopleCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to import person '{person.Name}': {ex.Message}");
                    }
                }
            }

            // Import custom lists
            if (exportData.CustomLists != null)
            {
                foreach (var list in exportData.CustomLists)
                {
                    try
                    {
                        var doc = CustomMediaListDocument.Create(cmd.UserId, cmd.TenantId, list.Title, list.Description);
                        if (list.Items != null)
                        {
                            foreach (var itm in list.Items)
                            {
                                doc.AddItem(itm.TmdbId, itm.Type, itm.Title, itm.PosterPath, itm.Year);
                            }
                        }
                        await _uow.CustomMediaLists.AddAsync(doc, ct);
                        listsCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to import list '{list.Title}': {ex.Message}");
                    }
                }
            }

            return Result.Success(new MediaImportResultDto(
                true,
                watchItemsCount,
                peopleCount,
                listsCount,
                errors
            ));
        }
        catch (Exception ex)
        {
            return Result.Failure<MediaImportResultDto>($"Failed to parse import data: {ex.Message}");
        }
    }
}
