namespace NotificationServiceAPI.Entities
{
    public enum NotificationType
    {
        TicketCreated,
        TicketAssigned,
        TicketStatusChanged,
        TicketCommentAdded,
        TicketResolved
    }

    public enum RecipientType
    {
        User,
        Agent,
        Admin
    }

    public class Notification
    {
        public int Id { get; set; }

        public string RecipientId { get; set; }   

        public RecipientType RecipientType { get; set; }

        public int TicketId { get; set; }

        public string TicketTitle { get; set; }

        public string Description { get; set; }

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }
    }
}