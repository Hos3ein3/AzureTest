using System.Threading;
using System.Threading.Tasks;


namespace AzureTest.Application.Messaging;

public interface ICommand
{
}

public interface ICommand<out TResponse>
{
}
