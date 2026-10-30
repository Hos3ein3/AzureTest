using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Queries.GetAllUsers;
using AzureTest.Contracts.DTOs.AuditLogs;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.AuditLog;

public static class GetAuditLogs
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetAuditLogsFilterDto filter,
        [FromServices] IAuditLogService auditLogService,
        [FromServices] ICurrentUser currentUser,
        [FromServices] ITenantProvider tenantProvider,
        [FromServices] IUserQueryUnit userQueries,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 5, 100);

        var isAdmin = currentUser.IsAdmin;
        var currentUserId = currentUser.Id;
        var currentTenantId = tenantProvider.CurrentTenantId;

        IReadOnlyList<UserListDto> availableUsers = Array.Empty<UserListDto>();
        if (isAdmin)
        {
            try
            {
                var usersResult = await userQueries.GetAllUsers.HandleAsync(
                    new GetAllUsersQuery(new GetAllUsersFilters { PageSize = 200 }),
                    cancellationToken);
                availableUsers = usersResult?.Items ?? Array.Empty<UserListDto>();
            }
            catch
            {
                // Fallback gracefully if query fails
            }
        }

        var (logs, totalCount) = await auditLogService.GetLogsAsync(
            isAdmin,
            currentUserId,
            currentTenantId,
            filter.ActionType,
            filter.SearchTerm,
            pageNumber,
            pageSize,
            filter.FilterUserId,
            cancellationToken);

        var logsDto = logs.Select(l => new AuditLogDto(
            l.Id,
            l.TenantId,
            l.UserId,
            l.UserName,
            l.UserEmail,
            l.ActionType,
            l.ActionName,
            l.Payload,
            l.ExecutionDurationMs,
            l.IsSuccess,
            l.ErrorMessage,
            l.IpAddress,
            l.ImpersonatedByAdminId,
            l.Timestamp
        )).ToList();

        var response = new AuditLogListResponseDto
        {
            Logs = logsDto,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActionType = filter.ActionType,
            SearchTerm = filter.SearchTerm,
            FilterUserId = filter.FilterUserId,
            AvailableUsers = availableUsers,
            IsAdmin = isAdmin,
            IsImpersonating = tenantProvider.IsAdminImpersonating,
            CurrentTenantId = currentTenantId,
            CurrentUserId = currentUserId
        };

        return Results.Ok(response);
    }
}
