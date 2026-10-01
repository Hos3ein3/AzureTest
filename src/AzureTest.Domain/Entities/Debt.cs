

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class Debt : BaseEntity, IAudit, ISoftDelete
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateOfBorrow { get; set; }
    public DateTime? DateOfReturn { get; set; }
    public DateTime? PromissedReturnDate { get; set; }
    public decimal Profit { get; set; }
    public Guid DebtPersonId { get; set; }
    public string? Description { get; set; }
    public bool IsDone { get; set; }

    // IAudit properties
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
}
