using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.ExpenseCategories.Commands;

public record CreateExpenseCategoryCommand(Guid UserId, string Name) : ICommand<Guid>;

public class CreateExpenseCategoryCommandHandler : ICommandHandler<CreateExpenseCategoryCommand, Guid>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public CreateExpenseCategoryCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<Guid> HandleAsync(CreateExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ExpenseCategory
        {
            UserId = request.UserId,
            Name = request.Name
        };

        await _noSqlUnitOfWork.ExpenseCategories.AddAsync(category);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync($"ExpCats_{request.UserId}");

        return category.Id;
    }
}

public record UpdateExpenseCategoryCommand(Guid Id, Guid UserId, string Name) : ICommand;

public class UpdateExpenseCategoryCommandHandler : ICommandHandler<UpdateExpenseCategoryCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public UpdateExpenseCategoryCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task HandleAsync(UpdateExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _noSqlUnitOfWork.ExpenseCategories.GetByIdAsync(request.Id);
        if (category == null || category.UserId != request.UserId) return;

        category.Name = request.Name;
        await _noSqlUnitOfWork.ExpenseCategories.UpdateAsync(request.Id, category);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync($"ExpCats_{request.UserId}");
    }
}

public record DeleteExpenseCategoryCommand(Guid Id, Guid UserId) : ICommand;

public class DeleteExpenseCategoryCommandHandler : ICommandHandler<DeleteExpenseCategoryCommand>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public DeleteExpenseCategoryCommandHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task HandleAsync(DeleteExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _noSqlUnitOfWork.ExpenseCategories.GetByIdAsync(request.Id);
        if (category == null || category.UserId != request.UserId) return;

        await _noSqlUnitOfWork.ExpenseCategories.RemoveAsync(request.Id);
        await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync($"ExpCats_{request.UserId}");
    }
}
