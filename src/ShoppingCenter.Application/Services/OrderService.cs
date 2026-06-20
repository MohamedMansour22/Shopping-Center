using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly INotificationService _notifications;
    private readonly IEmailSender _email;

    public OrderService(
        IOrderRepository orders,
        IProductRepository products,
        INotificationService notifications,
        IEmailSender email)
    {
        _orders = orders;
        _products = products;
        _notifications = notifications;
        _email = email;
    }

    public async Task<(OrderDto? Order, string? Error)> CreateAsync(
        CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Items.Count == 0)
            return (null, "Your order must contain at least one item.");

        // Collapse duplicate product lines so a product can't appear twice in one order.
        var quantities = new Dictionary<Guid, int>();
        foreach (var item in dto.Items)
        {
            if (item.Quantity < 1)
                return (null, "Each item quantity must be at least 1.");
            quantities[item.ProductId] = quantities.GetValueOrDefault(item.ProductId) + item.Quantity;
        }

        var order = new Order
        {
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            ShippingAddress = dto.ShippingAddress.Trim()
        };

        // Load the products TRACKED so decrementing their stock is persisted alongside the order.
        var products = await _products.GetByIdsForUpdateAsync(quantities.Keys.ToList(), cancellationToken);
        var byId = products.ToDictionary(p => p.Id);

        foreach (var (productId, quantity) in quantities)
        {
            if (!byId.TryGetValue(productId, out var product) || product.IsHidden)
                return (null, "One or more products in your cart are no longer available.");

            // Reject orders that exceed available stock (the only place stock is authoritatively checked).
            if (product.StockQuantity < quantity)
                return (null, $"Not enough stock for \"{product.Name}\" — only {product.StockQuantity} left.");

            // Snapshot the current name/price (never trust client-supplied prices) and reserve the stock.
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
            product.StockQuantity -= quantity;
        }

        order.TotalAmount = order.Items.Sum(i => i.LineTotal);

        // AddAsync's SaveChanges flushes the new order AND the tracked stock decrements in one transaction.
        var saved = await _orders.AddAsync(order, cancellationToken);

        // Notify admins AFTER the order is persisted (persist a feed row + push). The service swallows
        // its own errors, so a notification failure can never turn a successful order into a failure.
        await _notifications.RecordOrderPlacedAsync(saved, cancellationToken);

        // Email the customer their confirmation. Also best-effort (swallows its own errors) and a
        // no-op until SMTP is configured, so it can never break a successful order.
        await _email.SendOrderConfirmationAsync(saved, cancellationToken);

        return (ToDto(saved), null);
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _orders.GetPagedAsync(page, pageSize, cancellationToken);
        return new PagedResult<OrderDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasMore = (long)page * pageSize < totalCount
        };
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<IReadOnlyList<OrderStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        var statuses = await _orders.GetStatusesAsync(cancellationToken);
        return statuses.Select(s => new OrderStatusDto { Id = s.Id, Name = s.Name }).ToList();
    }

    // error == "NotFound" -> order doesn't exist; any other non-null error -> bad request message.
    public async Task<(OrderDto? Order, string? Error)> UpdateStatusAsync(
        Guid orderId, int statusId, CancellationToken cancellationToken = default)
    {
        var statuses = await _orders.GetStatusesAsync(cancellationToken);
        if (statuses.All(s => s.Id != statusId))
            return (null, "Unknown order status.");

        var updated = await _orders.UpdateStatusAsync(orderId, statusId, cancellationToken);
        return updated is null ? (null, "NotFound") : (ToDto(updated), null);
    }

    private static OrderDto ToDto(Order o) => new()
    {
        Id = o.Id,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        ShippingAddress = o.ShippingAddress,
        TotalAmount = o.TotalAmount,
        ItemCount = o.Items.Sum(i => i.Quantity),
        StatusId = o.StatusId,
        Status = o.Status?.Name ?? OrderStatusIds.NameOf(o.StatusId),
        Items = o.Items
            .Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            })
            .ToList(),
        CreatedAtUtc = o.CreatedAtUtc
    };
}
