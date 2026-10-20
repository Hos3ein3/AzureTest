

using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class Ticket : AggregateRoot, ISoftDelete, IAudit, ITenantEntity
{
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }

    public ICollection<TicketMessage> Messages { get; private set; } = new List<TicketMessage>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Ticket() { }

    public static Ticket Create(string subject, string content, TicketPriority priority, Guid userId, Guid? tenantId = null)
    {
        return new Ticket
        {
            Id = Guid.NewGuid(),
            Subject = subject,
            Content = content,
            Priority = priority,
            Status = TicketStatus.Open,
            UserId = userId,
            TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId
        };
    }

    public void UpdateStatus(TicketStatus status)
    {
        Status = status;
    }
}
