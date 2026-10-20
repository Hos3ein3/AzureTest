

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public sealed class Expense : BaseEntity, IAudit, ISoftDelete, ITenantEntity
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }

    // IAudit properties
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private Expense() { }

    private Expense(Guid userId, decimal amount, DateTime date, string description, Guid? categoryId, Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        Amount = amount;
        Date = date;
        Description = description;
        CategoryId = categoryId;
    }

    public static Expense Create(Guid userId, decimal amount, DateTime date, string description, Guid? categoryId, Guid? tenantId = null)
    {
        return new Expense(userId, amount, date, description, categoryId, tenantId);
    }

    public void UpdateDetails(decimal amount, DateTime date, string description, Guid? categoryId)
    {
        Amount = amount;
        Date = date;
        Description = description;
        CategoryId = categoryId;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
