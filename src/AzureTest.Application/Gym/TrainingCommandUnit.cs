using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents.Gym;
using AzureTest.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Gym;

public record CreateTrainingCommand(string Name, string? Description, string? MediaUrl, TrainingType Type, double? BaseCaloriesBurn, System.Collections.Generic.List<string>? Equipments=null) : ICommand<Result<Guid>>;
public class CreateTrainingCommandValidator : AbstractValidator<CreateTrainingCommand> {
    public CreateTrainingCommandValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class CreateTrainingCommandHandler : ICommandHandler<CreateTrainingCommand, Result<Guid>> {
    private readonly INoSqlUnitOfWork _uow;
    public CreateTrainingCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result<Guid>> HandleAsync(CreateTrainingCommand request, CancellationToken ct = default) {
        var training = new Training { Name = request.Name, Description = request.Description, MediaUrl = request.MediaUrl, Type = request.Type, BaseCaloriesBurn = request.BaseCaloriesBurn, Equipments = request.Equipments };
        await _uow.Trainings.AddAsync(training);
        return Result.Success(training.Id);
    }
}

public record UpdateTrainingCommand(Guid Id, string Name, string Description, string MediaUrl, TrainingType Type, double BaseCaloriesBurn, System.Collections.Generic.List<string> Equipments) : ICommand<Result>;
public class UpdateTrainingCommandValidator : AbstractValidator<UpdateTrainingCommand> {
    public UpdateTrainingCommandValidator() {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
public class UpdateTrainingCommandHandler : ICommandHandler<UpdateTrainingCommand, Result> {
    private readonly INoSqlUnitOfWork _uow;
    public UpdateTrainingCommandHandler(INoSqlUnitOfWork uow) { _uow = uow; }
    public async Task<Result> HandleAsync(UpdateTrainingCommand request, CancellationToken ct = default) {
        var training = await _uow.Trainings.GetByIdAsync(request.Id);
        if (training == null) return Result.Failure("Not found");
        training.Name = request.Name;
        training.Description = request.Description;
        training.MediaUrl = request.MediaUrl;
        training.Type = request.Type;
        training.BaseCaloriesBurn = request.BaseCaloriesBurn;
        training.Equipments = request.Equipments ?? new();
        await _uow.Trainings.UpdateAsync(request.Id, training);
        return Result.Success();
    }
}
