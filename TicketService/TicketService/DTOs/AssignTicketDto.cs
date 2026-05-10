namespace TicketService.DTOs
{
    public class AssignTicketDto
    {
        public string AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;

        public string Priority { get; set; }
    }
}
