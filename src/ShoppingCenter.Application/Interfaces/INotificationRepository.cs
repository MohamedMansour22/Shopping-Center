using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    // Most recent notifications (newest first), capped at take.
    Task<IReadOnlyList<Notification>> GetRecentAsync(int take, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    // Marks a single notification read; returns false if no such row.
    Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(CancellationToken cancellationToken = default);
}
