using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record DeleteVenueCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteVenueCommandHandler : BaseCommandHandler<DeleteVenueCommand, Result>
{
    public DeleteVenueCommandHandler(IUnitOfWork uow, ILogger<DeleteVenueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(DeleteVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await UnitOfWork.Venues.GetByIdAsync(request.Id);
        if (venue == null)
            return Result.Failure("Venue not found.");

        var linkedTeams = await UnitOfWork.Teams.FindAsync(t => t.VenueId == request.Id);
        foreach (var team in linkedTeams)
        {
            team.Update(
                team.Name,
                team.ShortName,
                team.TeamType,
                team.CountryId,
                team.Code,
                team.FoundedYear,
                team.StadiumName,
                team.StadiumCapacity,
                team.LogoUrl,
                null
            );
            UnitOfWork.Teams.Update(team);
        }

        UnitOfWork.Venues.Remove(venue);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
