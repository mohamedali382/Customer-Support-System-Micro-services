namespace NotificationServiceAPI.DTOs
{
    public record NotificationResponse(
        int Id,
        string RecipientId,
        string RecipientType,
        int TicketId,
        string TicketTitle,
        string Type,
        string Description,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt
    );
}