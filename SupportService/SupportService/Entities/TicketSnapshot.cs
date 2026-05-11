public class TicketSnapshot
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public string UserId { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Status { get; set; } = "Open";

    public string Description { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}