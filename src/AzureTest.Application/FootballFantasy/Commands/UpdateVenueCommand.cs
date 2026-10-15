using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.FootballFantasy.Commands;

public sealed record UpdateVenueCommand(
    Guid Id,
    string Name,
    Guid CountryId,
    string? City = null,
    string? Address = null,
    int? Capacity = null,
    string? Surface = null,
    string? ImageUrl = null
) : ICommand<Result>;

public sealed class UpdateVenueCommandValidator : AbstractValidator<UpdateVenueCommand>
{
    public UpdateVenueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(Venue.Rules.NameMinLength)
            .MaximumLength(Venue.Rules.NameMaxLength);

        RuleFor(x => x.CountryId)
            .NotEmpty();

        RuleFor(x => x.City)
            .MaximumLength(Venue.Rules.CityMaxLength);

        RuleFor(x => x.Address)
            .MaximumLength(Venue.Rules.AddressMaxLength);

        RuleFor(x => x.Surface)
            .MaximumLength(Venue.Rules.SurfaceMaxLength);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(Venue.Rules.ImageUrlMaxLength);

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(Venue.Rules.CapacityMin)
            .When(x => x.Capacity.HasValue);
    }
}

public sealed class UpdateVenueCommandHandler : BaseCommandHandler<UpdateVenueCommand, Result>
{
    public UpdateVenueCommandHandler(IUnitOfWork uow, ILogger<UpdateVenueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result> ExecuteAsync(UpdateVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await UnitOfWork.Venues.GetByIdAsync(request.Id);
        if (venue == null)
            return Result.Failure("Venue not found.");

        var country = await UnitOfWork.Countries.GetByIdAsync(request.CountryId);
        if (country == null)
            return Result.Failure("Associated country was not found.");

        venue.Update(
            request.Name,
            request.CountryId,
            request.City,
            request.Address,
            request.Capacity,
            request.Surface,
            request.ImageUrl
        );

        UnitOfWork.Venues.Update(venue);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
