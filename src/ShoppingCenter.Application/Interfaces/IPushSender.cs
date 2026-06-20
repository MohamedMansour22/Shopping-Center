using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

// Delivers a push notification to registered admin devices (Firebase Cloud Messaging).
public interface IPushSender
{
    // Push a "new order" notification to every registered admin device.
    // Implementations MUST swallow their own failures — a delivery problem
    // must never break order placement (POST /api/orders is public).
    Task NotifyOrderCreatedAsync(Order order, CancellationToken cancellationToken = default);
}
