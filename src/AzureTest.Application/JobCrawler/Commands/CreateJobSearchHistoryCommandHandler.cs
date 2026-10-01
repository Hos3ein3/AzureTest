using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using FluentValidation;

namespace AzureTest.Application.JobCrawler.Commands;

public record CreateJobSearchHistoryCommand(Guid UserId, string Position, string Location, string DatePosted, string Source) : ICommand<string>;

public class CreateJobSearchHistoryCommandValidator : AbstractValidator<CreateJobSearchHistoryCommand>
{
    public CreateJobSearchHistoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Position).NotEmpty();
    }
}

public class CreateJobSearchHistoryCommandHandler : ICommandHandler<CreateJobSearchHistoryCommand, string>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public CreateJobSearchHistoryCommandHandler(INoSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> HandleAsync(CreateJobSearchHistoryCommand command, CancellationToken cancellationToken = default)
    {
        var history = new JobSearchHistory
        {
            UserId = command.UserId,
            Position = command.Position,
            Location = command.Location,
            DatePosted = command.DatePosted,
            Source = command.Source,
            SearchDate = DateTime.UtcNow
        };

        await _unitOfWork.JobSearchHistories.AddAsync(history, cancellationToken);
        return history.Id.ToString();
    }
}
