
using System.Collections.Generic;

namespace NotificationServiceAPI.DTOs
{
    public record NotificationListResponse(
        List<NotificationResponse> Notifications,
        int TotalCount,
        int UnreadCount
    );
}
