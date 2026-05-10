namespace TicketService.DTOs
{
    public class TicketDtoResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; } =  DateTime.UtcNow;
        public DateTime Updated { get; set; }
        public string Status { get; set; }
    }
}
