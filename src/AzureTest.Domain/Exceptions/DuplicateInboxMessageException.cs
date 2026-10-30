namespace AzureTest.Domain.Exceptions;

public sealed class DuplicateInboxMessageException : Exception
{
    public Guid EventId { get; }
    public string HandlerName { get; }

    public DuplicateInboxMessageException(Guid eventId, string handlerName, Exception? innerException = null)
        : base($"Inbox message for event '{eventId}' and handler '{handlerName}' already exists.", innerException)
    {
        EventId = eventId;
        HandlerName = handlerName;
    }
}