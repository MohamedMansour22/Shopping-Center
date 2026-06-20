using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;

namespace ShoppingCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private const int DefaultTake = 20;
    private const int MaxTake = 50;

    private readonly IDeviceTokenRepository _tokens;
    private readonly INotificationService _notifications;

    public NotificationsController(IDeviceTokenRepository tokens, INotificationService notifications)
    {
        _tokens = tokens;
        _notifications = notifications;
    }

    // Admin: the most recent notifications for the in-app feed (newest first).
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetRecent(
        [FromQuery] int take = DefaultTake, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, MaxTake);
        var items = await _notifications.GetRecentAsync(take, ct);
        return Ok(items);
    }

    // Admin: count of unread notifications (bell badge).
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount(CancellationToken ct)
    {
        var count = await _notifications.GetUnreadCountAsync(ct);
        return Ok(new { count });
    }

    // Admin: mark a single notification read.
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var found = await _notifications.MarkReadAsync(id, ct);
        return found ? NoContent() : NotFound();
    }

    // Admin: mark every notification read.
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _notifications.MarkAllReadAsync(ct);
        return NoContent();
    }

    // Admin: register this browser's FCM token to receive order notifications.
    [HttpPost("device-tokens")]
    public async Task<IActionResult> Register(RegisterDeviceTokenDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new { message = "A device token is required." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _tokens.AddOrUpdateAsync(dto.Token, userId, ct);
        return NoContent();
    }

    // Admin: unregister this browser's token (e.g. on logout).
    [HttpDelete("device-tokens")]
    public async Task<IActionResult> Unregister(RegisterDeviceTokenDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new { message = "A device token is required." });

        await _tokens.RemoveAsync(dto.Token, ct);
        return NoContent();
    }
}
