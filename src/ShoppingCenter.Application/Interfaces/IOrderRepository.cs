using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);

    // Admin list, server-side paginated: one page of orders (newest first) plus the total count.
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);

    // A single order with its items and status, or null if not found.
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // The available order statuses (lookup rows), ordered by id.
    Task<IReadOnlyList<OrderStatus>> GetStatusesAsync(CancellationToken cancellationToken = default);

    // Sets an order's status. Returns the updated order (with status loaded), or null if not found.
    Task<Order?> UpdateStatusAsync(Guid orderId, int statusId, CancellationToken cancellationToken = default);
}
