namespace SupportService.Entities
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Agent Agent { get; set; } = null!;
    }
}
