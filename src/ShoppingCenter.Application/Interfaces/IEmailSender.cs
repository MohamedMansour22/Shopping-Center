using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

// Sends transactional email to customers (e.g. order confirmations).
public interface IEmailSender
{
    // Email the customer an order confirmation for a freshly placed order.
    // Implementations MUST swallow their own failures — a delivery problem
    // must never break order placement (POST /api/orders is public), and the
    // order is already persisted by the time this is called.
    Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default);
}
