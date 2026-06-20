namespace ShoppingCenter.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Customer details captured at checkout (no customer accounts — these live on the order).
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    // Snapshot of the order total at the time it was placed (sum of its line totals).
    public decimal TotalAmount { get; set; }

    // Lifecycle status (FK to the OrderStatus lookup); new orders start as Placed.
    public int StatusId { get; set; } = OrderStatusIds.Placed;
    public OrderStatus? Status { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
