using System.Globalization;
using Microsoft.Extensions.Logging;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IPushSender _push;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        IPushSender push,
        ILogger<NotificationService> logger)
    {
        _repo = repo;
        _push = push;
        _logger = logger;
    }

    public async Task RecordOrderPlacedAsync(Order order, CancellationToken cancellationToken = default)
    {
        // Best-effort: the order is already persisted by the time we get here, so a notification
        // failure must never bubble up and turn a successful order into an error.
        try
        {
            var total = order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-US"));
            var notification = new Notification
            {
                Type = NotificationTypes.OrderPlaced,
                Title = "New order received",
                Message = $"{order.CustomerName} • {total}",
                OrderId = order.Id
            };
            await _repo.AddAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist order-placed notification for order {OrderId}.", order.Id);
        }

        // Push delivery already swallows its own errors.
        await _push.NotifyOrderCreatedAsync(order, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var items = await _repo.GetRecentAsync(take, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        => _repo.GetUnreadCountAsync(cancellationToken);

    public Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
        => _repo.MarkReadAsync(id, cancellationToken);

    public Task MarkAllReadAsync(CancellationToken cancellationToken = default)
        => _repo.MarkAllReadAsync(cancellationToken);

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        OrderId = n.OrderId,
        IsRead = n.IsRead,
        CreatedAtUtc = n.CreatedAtUtc
    };
}
