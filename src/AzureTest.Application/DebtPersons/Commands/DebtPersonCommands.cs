using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.DebtPersons.Commands;

public record CreateDebtPersonCommand(Guid UserId, string Name) : ICommand<Result<Guid>>;

public class CreateDebtPersonCommandHandler : ICommandHandler<CreateDebtPersonCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public CreateDebtPersonCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<Result<Guid>> HandleAsync(CreateDebtPersonCommand request, CancellationToken cancellationToken)
    {
        var person = new DebtPerson
        {
            UserId = request.UserId,
            FullName = request.Name
        };

        await _noSqlUnitOfWork.DebtPersons.AddAsync(person);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        _redisService.GenerateKey(IRedisService.DebtPersons_Key, request.UserId.ToString());
        await _redisService.RemoveAsync($"DebtPersons_{request.UserId}");

        return Result.Success(person.Id);
    }
}

public record UpdateDebtPersonCommand(Guid Id, Guid UserId, string Name) : ICommand;

public class UpdateDebtPersonCommandHandler : ICommandHandler<UpdateDebtPersonCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public UpdateDebtPersonCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<Result> HandleAsync(UpdateDebtPersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(request.Id);
        if (person == null || person.UserId != request.UserId) return Result.Failure("Error");

        person.FullName = request.Name;
        await _noSqlUnitOfWork.DebtPersons.UpdateAsync(request.Id, person);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync($"DebtPersons_{request.UserId}");
        return Result.Success();
    }
}

public record DeleteDebtPersonCommand(Guid Id, Guid UserId) : ICommand;

public class DeleteDebtPersonCommandHandler : ICommandHandler<DeleteDebtPersonCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public DeleteDebtPersonCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<Result> HandleAsync(DeleteDebtPersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(request.Id);
        if (person == null || person.UserId != request.UserId) return Result.Failure("Error");

        await _noSqlUnitOfWork.DebtPersons.RemoveAsync(request.Id);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync($"DebtPersons_{request.UserId}");
        return Result.Success();
    }
}
