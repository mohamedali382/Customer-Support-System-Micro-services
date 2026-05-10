namespace SupportService.DTOs.AssignmentDTOs
{
    public class AssignmentResponse
    {
        public AssignmentResponse(int id, int ticketId, string agentId, string agentName, string assignedBy, DateTime assignedAt, DateTime? resolvedAt, bool isActive, string notes)
        {
            Id = id;
            TicketId = ticketId;
            AgentId = agentId;
            AgentName = agentName;
            AssignedBy = assignedBy;
            AssignedAt = assignedAt;
            ResolvedAt = resolvedAt;
            IsActive = isActive;
            Notes = notes;
        }

        public int Id { get; set; }

        public int TicketId { get; set; }

        public string AgentId { get; set; } = string.Empty;

        public string AgentName { get; set; } = string.Empty;

        public string AssignedBy { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public bool IsActive { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}