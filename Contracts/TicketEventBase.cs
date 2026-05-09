using System;

namespace Contracts;

public class TicketEventBase
{
    public int TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? AgentId { get; set; }
    public string? AgentName { get; set; }
}