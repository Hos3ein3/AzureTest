

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class TicketMessage : BaseEntity, IAudit, ISoftDelete, ITenantEntity
{
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsReadByCreator { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private TicketMessage() { }

    public static TicketMessage Create(Guid ticketId, Guid userId, string content, Guid? tenantId = null)
    {
        return new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId,
            TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId,
            Content = content,
            IsReadByCreator = false
        };
    }

    public void MarkAsRead()
    {
        IsReadByCreator = true;
    }
}
