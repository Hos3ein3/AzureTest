using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class RolePermission : BaseEntity
{
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
