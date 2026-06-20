using Microsoft.EntityFrameworkCore;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Data;

namespace ShoppingCenter.Infrastructure.Repositories;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly AppDbContext _db;

    public DeviceTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddOrUpdateAsync(string token, string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
        if (existing is null)
        {
            _db.DeviceTokens.Add(new DeviceToken { Token = token, UserId = userId });
        }
        else
        {
            // The same browser can re-register the same token under a different admin / session.
            existing.UserId = userId;
            existing.CreatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string token, CancellationToken cancellationToken = default)
    {
        await _db.DeviceTokens
            .Where(t => t.Token == token)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DeviceTokens
            .AsNoTracking()
            .Select(t => t.Token)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveRangeAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default)
    {
        var list = tokens.ToList();
        if (list.Count == 0)
            return;

        await _db.DeviceTokens
            .Where(t => list.Contains(t.Token))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
