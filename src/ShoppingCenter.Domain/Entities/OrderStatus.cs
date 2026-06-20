namespace ShoppingCenter.Domain.Entities;

// Lookup table for the lifecycle state of an order. Rows are fixed/seeded (see OrderStatusIds).
public class OrderStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// Stable ids for the seeded statuses, referenced from code (defaults, seeding, validation).
public static class OrderStatusIds
{
    public const int Placed = 1;
    public const int Delivered = 2;

    public static string NameOf(int id) => id switch
    {
        Placed => "Placed",
        Delivered => "Delivered",
        _ => "Unknown"
    };
}
