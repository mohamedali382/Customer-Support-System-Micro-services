namespace Contracts;

public class TicketCommentAddedEvent : TicketEventBase
{
    public string AuthorName { get; set; } = string.Empty;
    public bool IsAgentReply { get; set; }

    public TicketCommentAddedEvent()
    {
        EventType = EventType.TicketCommentAdded;
    }
}