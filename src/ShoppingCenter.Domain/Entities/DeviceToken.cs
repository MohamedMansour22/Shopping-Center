namespace ShoppingCenter.Domain.Entities;

// An FCM registration token for a browser/device that should receive push notifications.
// Registration is admin-authenticated, so every stored token belongs to an admin user.
public class DeviceToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // The FCM registration token issued to the client by Firebase Messaging (unique).
    public string Token { get; set; } = string.Empty;

    // The Identity user id (admin) that registered this token, for housekeeping.
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
