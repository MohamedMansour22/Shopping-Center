namespace ShoppingCenter.Domain.Entities;

// A persisted admin notification (the in-app notification feed). Created when an order is placed;
// also delivered as an FCM push. OrderId is the click target (order details screen).
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Discriminates the kind of notification (see NotificationTypes).
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // The order this notification links to (nullable so other notification types can omit it).
    public Guid? OrderId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class NotificationTypes
{
    public const string OrderPlaced = "OrderPlaced";
}
