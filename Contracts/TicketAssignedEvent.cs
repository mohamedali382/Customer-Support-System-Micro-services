namespace Contracts;

public class TicketAssignedEvent : TicketEventBase
{
    public TicketAssignedEvent()
    {
        EventType = EventType.TicketAssigned;
    }
}