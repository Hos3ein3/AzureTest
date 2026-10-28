using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public sealed class UserPermission : BaseEntity
{
    public static class Rules
    {
        public const bool DefaultIsGranted = true;
    }

    private UserPermission() { }

    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public bool IsGranted { get; private set; } = Rules.DefaultIsGranted;
    public Permission Permission { get; private set; } = null!;

    public static UserPermission Create(Guid userId, Guid permissionId, bool isGranted = Rules.DefaultIsGranted)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException("Permission ID cannot be empty.", nameof(permissionId));
        }

        return new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId,
            IsGranted = isGranted
        };
    }

    public void Grant()
    {
        IsGranted = true;
    }

    public void Revoke()
    {
        IsGranted = false;
    }
}
