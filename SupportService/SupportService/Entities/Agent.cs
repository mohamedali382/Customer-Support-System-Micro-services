namespace SupportService.Entities
{
    public enum AgentStatus
    {
        Available,
        Busy,
        Offline
    }

    public class Agent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public AgentStatus Status { get; set; } = AgentStatus.Available;
        public string Department { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<TicketAssignment> Assignments { get; set; } = new();
    }
}
