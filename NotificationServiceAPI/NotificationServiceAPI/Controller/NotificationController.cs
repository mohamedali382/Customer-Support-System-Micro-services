using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NotificationServiceAPI.Services.Interfaces;
using NotificationServiceAPI.Entities;
using NotificationServiceAPI.Services;

namespace NotificationServiceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly InotificationService _notificationService;

    public NotificationsController(InotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var recipientType = Enum.Parse<RecipientType>(role, true);

        var result = await _notificationService
            .GetByRecipientIdAsync(userId, recipientType, unreadOnly);

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var recipientType = Enum.Parse<RecipientType>(role, true);

        var count = await _notificationService
            .GetUnreadCountAsync(userId, recipientType);

        return Ok(new { userId, unreadCount = count });
    }

    [HttpPatch("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var recipientType = Enum.Parse<RecipientType>(role, true);

        var result = await _notificationService
            .MarkAsReadAsync(notificationId, userId, recipientType);

        if (!result)
            return NotFound();

        return Ok(new { message = "Marked as read." });
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var recipientType = Enum.Parse<RecipientType>(role, true);

        await _notificationService
            .MarkAllAsReadAsync(userId, recipientType);

        return Ok(new { message = "All notifications marked as read." });
    }

    [HttpDelete("{notificationId:int}")]
    public async Task<IActionResult> Delete(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var recipientType = Enum.Parse<RecipientType>(role, true);

        var result = await _notificationService
            .DeleteAsync(notificationId, userId, recipientType);

        if (!result)
            return NotFound();

        return NoContent();
    }
}