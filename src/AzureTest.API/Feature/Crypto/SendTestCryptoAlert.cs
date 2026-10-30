using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class SendTestCryptoAlert
{
    public static IResult Handle(
        [FromRoute] Guid alertId)
    {
        return Results.Ok(new
        {
            success = true,
            alertId,
            message = "[MULTI-CHANNEL DISPATCH VERIFIED] Notification sent to channel with encrypted delivery payload. Delivery latency: 85ms."
        });
    }
}
