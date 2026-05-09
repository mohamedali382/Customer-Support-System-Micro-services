namespace Contracts;

public class TicketStatusChangedEvent : TicketEventBase
{
    public string Status { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }

    public TicketStatusChangedEvent()
    {
        EventType = EventType.TicketStatusChanged;
    }
}