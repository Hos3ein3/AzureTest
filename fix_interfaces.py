import os
import re

# Update ICommand and ICommandHandler
content = """using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Messaging;

public interface ICommand
{
}

public interface ICommand<out TResponse>
{
}
"""

with open('src/AzureTest.Application/Messaging/ICommand.cs', 'w') as f:
    f.write(content)

handler_content = """using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Messaging;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
"""
with open('src/AzureTest.Application/Messaging/ICommandHandler.cs', 'w') as f:
    f.write(handler_content)
