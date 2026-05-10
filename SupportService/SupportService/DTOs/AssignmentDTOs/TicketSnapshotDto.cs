namespace SupportService.DTOs.AssignmentDTOs
{
    public class TicketSnapshotDto
    {
        public TicketSnapshotDto(int ticketId, string userId, string title, string status, DateTime createdAt)
        {
            TicketId = ticketId;
            UserId = userId;
            Title = title;
            Status = status;
            CreatedAt = createdAt;
        }

        public int TicketId { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    }
}
