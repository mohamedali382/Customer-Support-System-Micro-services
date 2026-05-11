namespace SupportService.DTOs.AssignmentDTOs
{
    public class AssignTicketRequest
    {
        public int TicketId { get; set; }

        public string AgentId { get; set; } = string.Empty;

        public string Titile { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;

        // Filled from JWT in controller/service
        public string? AssignedBy { get; set; }

        public string? Notes { get; set; }
    }
}
