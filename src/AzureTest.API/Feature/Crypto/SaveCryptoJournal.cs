using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.SaveCryptoJournal;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class SaveCryptoJournal
{
    public static async Task<IResult> HandleAsync(
        [FromBody] SaveCryptoJournalRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var command = new SaveCryptoJournalCommand(
            request.Id,
            request.PositionId,
            currentUser.Id,
            currentUser.TenantId,
            request.Symbol,
            request.StrategyTag,
            request.EmotionTag,
            request.Rating,
            request.Notes,
            request.LessonsLearned);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.SaveJournal.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
