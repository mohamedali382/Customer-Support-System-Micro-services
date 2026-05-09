namespace Contracts;

public class TicketCreatedEvent : TicketEventBase
{
    public string Description { get; set; } = string.Empty;

    public TicketCreatedEvent()
    {
        EventType = EventType.TicketCreated;
    }
}