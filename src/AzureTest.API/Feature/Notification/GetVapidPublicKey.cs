using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class GetVapidPublicKey
{
    public static IResult Handle(
        [FromServices] IWebPushService webPushService)
    {
        var key = webPushService.GetVapidPublicKey();
        return Results.Ok(new { publicKey = key });
    }
}
