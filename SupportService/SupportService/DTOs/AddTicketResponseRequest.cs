public class AddTicketResponseRequest
{
    public int TicketId { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Resolution { get; set; }
}