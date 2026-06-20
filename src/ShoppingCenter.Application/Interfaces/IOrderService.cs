using ShoppingCenter.Application.DTOs;

namespace ShoppingCenter.Application.Interfaces;

public interface IOrderService
{
    // Places an order. Prices/totals are computed server-side from the current product catalogue.
    // Returns (order, null) on success, or (null, error) if a referenced product can't be ordered.
    Task<(OrderDto? Order, string? Error)> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<OrderDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default);

    // Updates an order's status. Returns (order, null) on success, (null, error) if the order or
    // the target status doesn't exist.
    Task<(OrderDto? Order, string? Error)> UpdateStatusAsync(
        Guid orderId, int statusId, CancellationToken cancellationToken = default);
}
