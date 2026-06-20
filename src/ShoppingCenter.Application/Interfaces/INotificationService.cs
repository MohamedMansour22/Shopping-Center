using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

// The admin notification feed: persists notifications and exposes them to the admin UI.
public interface INotificationService
{
    // On order placed: persist a notification row AND deliver an FCM push. Best-effort —
    // must never throw into the order flow.
    Task RecordOrderPlacedAsync(Order order, CancellationToken cancellationToken = default);

    // Most recent notifications (newest first) for the admin feed.
    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int take, CancellationToken cancellationToken = default);

    // Count of unread notifications (for the bell badge).
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    // Mark one notification read. Returns false if it doesn't exist.
    Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken = default);

    // Mark every unread notification read.
    Task MarkAllReadAsync(CancellationToken cancellationToken = default);
}
