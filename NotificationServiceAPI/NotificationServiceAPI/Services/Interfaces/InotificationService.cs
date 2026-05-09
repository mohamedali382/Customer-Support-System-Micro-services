using NotificationServiceAPI.Entities;
using NotificationServiceAPI.DTOs;
using System.Threading.Tasks;
namespace NotificationServiceAPI.Services.Interfaces;

public interface InotificationService
{

    Task<Notification> CreateAsync(string recipientId, RecipientType recipientType, int ticketId, string ticketTitle, NotificationType type, string description);
    Task<NotificationListResponse> GetByRecipientIdAsync(string recipientId, RecipientType recipientType, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(string recipientId, RecipientType recipientType);
    Task<bool> MarkAsReadAsync(int notificationId, string recipientId, RecipientType recipientType);
    Task MarkAllAsReadAsync(string recipientId, RecipientType recipientType);
    Task<bool> DeleteAsync(int notificationId, string recipientId, RecipientType recipientType);
}