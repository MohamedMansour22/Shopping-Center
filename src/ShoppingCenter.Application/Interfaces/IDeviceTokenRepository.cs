using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

public interface IDeviceTokenRepository
{
    // Register a token, or refresh its owner/timestamp if it already exists (tokens are unique).
    Task AddOrUpdateAsync(string token, string userId, CancellationToken cancellationToken = default);

    // Remove a single token (e.g. on logout / when the client unsubscribes).
    Task RemoveAsync(string token, CancellationToken cancellationToken = default);

    // Every registered admin token — the recipients for an order notification.
    Task<IReadOnlyList<string>> GetAllTokensAsync(CancellationToken cancellationToken = default);

    // Prune tokens FCM reported as no longer valid so the table doesn't accumulate dead entries.
    Task RemoveRangeAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default);
}
