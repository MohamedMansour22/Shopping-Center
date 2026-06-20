using Microsoft.EntityFrameworkCore;
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
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.AsNoTracking();

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
