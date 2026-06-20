namespace ShoppingCenter.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    // Reference to the purchased product. Not a foreign key on purpose: the product may later be
    // edited or deleted, but the order must keep an accurate record of what was bought.
    public Guid ProductId { get; set; }

    // Name + unit price snapshotted at purchase time, so the order is stable if the product changes.
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
