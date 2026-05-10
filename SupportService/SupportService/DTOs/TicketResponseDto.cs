public class TicketResponseDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string TicketTitle { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
}