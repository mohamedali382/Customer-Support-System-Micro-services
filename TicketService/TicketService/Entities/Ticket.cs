namespace TicketService.Entities
{
    public class Ticket
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int? AssignedAgentId { get; set; }
        public string? AssignedAgentName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } = "medium";
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; }
        public string Status { get; set; }
        
    }
}
