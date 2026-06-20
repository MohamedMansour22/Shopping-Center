using Microsoft.EntityFrameworkCore;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Data;

namespace ShoppingCenter.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenByDescending(n => n.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Notifications.CountAsync(n => !n.IsRead, cancellationToken);
    }

    public async Task<bool> MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var updated = await _db.Notifications
            .Where(n => n.Id == id && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

        // Already-read rows update 0 but still "exist" — treat any existing row as success.
        if (updated > 0)
            return true;

        return await _db.Notifications.AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        await _db.Notifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }
}
