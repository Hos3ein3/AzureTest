using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.SaveJournalEntry;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class SaveForexJournalEntry
{
    public static async Task<IResult> HandleAsync(
        [FromBody] SaveJournalEntryCommand command,
        [FromServices] IForexCommandUnit commands,
        [FromServices] IValidationService validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.SaveJournalEntry.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
