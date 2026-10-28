using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Users;
using FluentValidation;

namespace AzureTest.Application.Auth.Queries.GetSessionUser;

public sealed record GetSessionUserQuery(Guid UserId) : IQuery<Result<UserSessionCacheDto?>>;

public sealed class GetSessionUserQueryValidator : AbstractValidator<GetSessionUserQuery>
{
    public GetSessionUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class GetSessionUserQueryHandler : IQueryHandler<GetSessionUserQuery, Result<UserSessionCacheDto?>>
{
    private readonly IRedisService _redisService;

    public GetSessionUserQueryHandler(IRedisService redisService)
    {
        _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
    }

    public async Task<Result<UserSessionCacheDto?>> HandleAsync(
        GetSessionUserQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<UserSessionCacheDto?>.Success(null);
        }

        var session = await _redisService.GetAsync<UserSessionCacheDto>($"User_Info_{request.UserId}");
        return Result<UserSessionCacheDto?>.Success(session);
    }
}
