namespace SupportService.Entities
{
    public class TicketAssignment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string AgentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; } = string.Empty;

        public Agent Agent { get; set; } = null!;
    }
}
