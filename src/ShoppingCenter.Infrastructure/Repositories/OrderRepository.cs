using Microsoft.EntityFrameworkCore;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Data;

namespace ShoppingCenter.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, OrderListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.AsNoTracking();

        // DateFrom/DateTo arrive as absolute instants: the client converts the operator's chosen
        // local calendar days into UTC boundaries (start-of-day, and exclusive start-of-next-day),
        // so the range matches the operator's day rather than the UTC day. Captured into locals so
        // EF parameterises them.
        if (filter.DateFrom is { } from)
        {
            var fromUtc = from.UtcDateTime;
            query = query.Where(o => o.CreatedAtUtc >= fromUtc);
        }
        if (filter.DateTo is { } toExclusive)
        {
            var toUtc = toExclusive.UtcDateTime;
            query = query.Where(o => o.CreatedAtUtc < toUtc);
        }
        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
        {
            var pattern = LikePattern.Contains(filter.CustomerName.Trim());
            query = query.Where(o => EF.Functions.Like(o.CustomerName, pattern));
        }
        if (filter.StatusId is { } statusId)
        {
            query = query.Where(o => o.StatusId == statusId);
        }

        // Count comes off the SAME filtered query, so TotalCount/HasMore match the filtered set.
        var totalCount = await query.CountAsync(cancellationToken);

        // ThenBy(Id) gives a stable total order so Skip/Take never duplicates or drops an order
        // when several share a CreatedAtUtc timestamp.
        var items = await query
            .Include(o => o.Items)
            .Include(o => o.Status)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ThenBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Status)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderStatus>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.OrderStatuses
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> UpdateStatusAsync(Guid orderId, int statusId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
            return null;

        order.StatusId = statusId;
        await _db.SaveChangesAsync(cancellationToken);

        // Load the new status row so the returned order carries its name.
        order.Status = await _db.OrderStatuses.FindAsync([statusId], cancellationToken);
        return order;
    }
}
