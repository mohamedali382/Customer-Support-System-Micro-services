namespace TicketService.DTOs
{
    public class TicketResolvedDtoResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
       
        public string Priority { get; set; } = string.Empty;
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
