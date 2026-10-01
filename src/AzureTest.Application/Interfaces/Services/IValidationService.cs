using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Interfaces.Services;

public interface IValidationService
{
    /// <summary>
    /// Validates the given model using registered FluentValidation validators.
    /// Returns a successful Result if valid, or a ValidationFailure Result if invalid.
    /// </summary>
    Task<Result> ValidateAsync<T>(T model, CancellationToken cancellationToken = default);
}
