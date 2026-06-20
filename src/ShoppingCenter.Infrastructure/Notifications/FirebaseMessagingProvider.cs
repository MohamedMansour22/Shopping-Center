using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ShoppingCenter.Infrastructure.Notifications;

// Owns the single FirebaseApp instance (FirebaseApp.Create may only be called once per process).
// Registered as a singleton; initialization is guarded so a missing/invalid credentials file
// leaves Messaging == null and the app boots normally with notifications disabled.
public class FirebaseMessagingProvider
{
    public FirebaseMessaging? Messaging { get; }

    public FirebaseMessagingProvider(IOptions<FirebaseSettings> options, ILogger<FirebaseMessagingProvider> logger)
    {
        var path = options.Value.CredentialsPath;

        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning(
                "Firebase credentials path is not configured (Firebase:CredentialsPath). " +
                "Push notifications are disabled.");
            return;
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            logger.LogWarning(
                "Firebase credentials file not found at '{Path}'. Push notifications are disabled.", fullPath);
            return;
        }

        try
        {
            var json = File.ReadAllText(fullPath);
            var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = CredentialFactory.FromJson<ServiceAccountCredential>(json).ToGoogleCredential()
            });
            Messaging = FirebaseMessaging.GetMessaging(app);
            logger.LogInformation("Firebase Messaging initialized from '{Path}'.", fullPath);
        }
        catch (Exception ex)
        {
            // A bad credentials file must not crash startup — just disable notifications.
            logger.LogError(ex, "Failed to initialize Firebase Messaging. Push notifications are disabled.");
        }
    }
}
