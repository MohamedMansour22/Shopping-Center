namespace ShoppingCenter.Application.DTOs;

// The FCM registration token the admin's browser obtained from Firebase Messaging.
public class RegisterDeviceTokenDto
{
    public string Token { get; set; } = string.Empty;
}

// A persisted admin notification, as shown in the in-app notification feed.
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
