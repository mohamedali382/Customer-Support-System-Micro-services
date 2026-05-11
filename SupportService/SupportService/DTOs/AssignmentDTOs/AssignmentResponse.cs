namespace SupportService.DTOs.AssignmentDTOs
{
    public class AssignmentResponse
    {
        public AssignmentResponse(int id, int ticketId, string agentId, string agentName, string description, string title, string assignedBy, DateTime assignedAt, DateTime? resolvedAt, bool isActive, string? notes)
        {
            Id = id;
            TicketId = ticketId;
            AgentId = agentId;
            AgentName = agentName;
            Description = description;
            Title = title;
            AssignedBy = assignedBy;
            AssignedAt = assignedAt;
            ResolvedAt = resolvedAt;
            IsActive = isActive;
            Notes = notes;
        }

        public AssignmentResponse(int id, int ticketId, string agentId, string agentName,string description, string assignedBy, string title, string assignedBy1, DateTime assignedAt, DateTime? resolvedAt, bool isActive, string notes)
        {
            Id = id;
            TicketId = ticketId;
            AgentId = agentId;
            AgentName = agentName;
            Description = description;
            Title = title;
            AssignedBy = assignedBy;
            AssignedAt = assignedAt;
            ResolvedAt = resolvedAt;
            IsActive = isActive;
            Notes = notes;
        }

        public int Id { get; set; }

        public int TicketId { get; set; }

        public string AgentId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string AgentName { get; set; } = string.Empty;

        public string AssignedBy { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public bool IsActive { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}