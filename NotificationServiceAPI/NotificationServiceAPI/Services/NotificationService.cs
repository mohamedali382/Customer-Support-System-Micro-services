using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationServiceAPI.Services.Interfaces;
using NotificationServiceAPI.Data;
using NotificationServiceAPI.DTOs;
using NotificationServiceAPI.Entities;

namespace NotificationServiceAPI.Services;

public class NotificationService : InotificationService
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(NotificationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification> CreateAsync(
        string recipientId,
        RecipientType recipientType,
        int ticketId,
        string ticketTitle,
        NotificationType type,
        string description)
    {
        if (string.IsNullOrWhiteSpace(recipientId))
            throw new ArgumentException("RecipientId is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.");

        if (string.IsNullOrWhiteSpace(ticketTitle))
            throw new ArgumentException("TicketTitle is required.");

        var notification = new Notification
        {
            RecipientId = recipientId,
            RecipientType = recipientType,
            TicketId = ticketId,
            TicketTitle = ticketTitle,
            Type = type,
            Description = description,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    public async Task<NotificationListResponse> GetByRecipientIdAsync(
        string recipientId,
        RecipientType recipientType,
        bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n =>
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var unreadCount = await _context.Notifications
            .CountAsync(n =>
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType &&
                !n.IsRead);

        return new NotificationListResponse(
            notifications.Select(MapToResponse).ToList(),
            notifications.Count,
            unreadCount
        );
    }
    public async Task<int> GetUnreadCountAsync(
        string recipientId,
        RecipientType recipientType)
    {
        return await _context.Notifications
            .CountAsync(n =>
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType &&
                !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        string recipientId,
        RecipientType recipientType)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == notificationId &&
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType);

        if (notification is null)
            return false;

        if (notification.IsRead)
            return true;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(
        string recipientId,
        RecipientType recipientType)
    {
        var unread = await _context.Notifications
            .Where(n =>
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType &&
                !n.IsRead)
            .ToListAsync();

        if (!unread.Any())
            return;

        var now = DateTime.UtcNow;

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Marked {Count} notifications as read for {RecipientType} {RecipientId}",
            unread.Count,
            recipientType,
            recipientId
        );
    }
    public async Task<bool> DeleteAsync(
        int notificationId,
        string recipientId,
        RecipientType recipientType)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == notificationId &&
                n.RecipientId == recipientId &&
                n.RecipientType == recipientType);

        if (notification is null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }


    private static NotificationResponse MapToResponse(Notification n)
        => new(
            n.Id,
            n.RecipientId,
            n.RecipientType.ToString(),
            n.TicketId,
            n.TicketTitle,
            n.Type.ToString(),
            n.Description,
            n.IsRead,
            n.CreatedAt,
            n.ReadAt
        );
}