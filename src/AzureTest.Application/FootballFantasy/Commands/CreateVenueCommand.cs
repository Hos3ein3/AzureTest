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

public sealed record CreateVenueCommand(
    string Name,
    Guid CountryId,
    string? City = null,
    string? Address = null,
    int? Capacity = null,
    string? Surface = null,
    string? ImageUrl = null
) : ICommand<Result<Guid>>;

public sealed class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueCommandValidator()
    {
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

public sealed class CreateVenueCommandHandler : BaseCommandHandler<CreateVenueCommand, Result<Guid>>
{
    public CreateVenueCommandHandler(IUnitOfWork uow, ILogger<CreateVenueCommandHandler> logger) : base(uow, logger) { }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        var country = await UnitOfWork.Countries.GetByIdAsync(request.CountryId);
        if (country == null)
            return Result<Guid>.Failure("Associated country was not found.");

        var venue = Venue.Create(
            request.Name,
            request.CountryId,
            request.City,
            request.Address,
            request.Capacity,
            request.Surface,
            request.ImageUrl
        );

        UnitOfWork.Venues.Add(venue);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(venue.Id);
    }
}
