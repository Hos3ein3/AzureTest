using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Notifications;
using FluentValidation;

namespace AzureTest.Application.Notifications.Commands.SubscribeWebPush;

public sealed record SubscribeWebPushCommand(
    Guid UserId,
    Guid TenantId,
    string Endpoint,
    string P256DH,
    string Auth
) : ICommand<Result>;

public sealed class SubscribeWebPushCommandValidator : AbstractValidator<SubscribeWebPushCommand>
{
    public SubscribeWebPushCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Endpoint).NotEmpty().WithMessage("Endpoint is required.");
    }
}

public sealed class SubscribeWebPushCommandHandler : ICommandHandler<SubscribeWebPushCommand, Result>
{
    private readonly IWebPushService _webPushService;

    public SubscribeWebPushCommandHandler(IWebPushService webPushService)
    {
        _webPushService = webPushService ?? throw new ArgumentNullException(nameof(webPushService));
    }

    public async Task<Result> HandleAsync(
        SubscribeWebPushCommand request,
        CancellationToken cancellationToken = default)
    {
        var dto = new WebPushSubscriptionDto(request.Endpoint, request.P256DH, request.Auth);
        return await _webPushService.SubscribeAsync(request.UserId, request.TenantId, dto, cancellationToken);
    }
}
