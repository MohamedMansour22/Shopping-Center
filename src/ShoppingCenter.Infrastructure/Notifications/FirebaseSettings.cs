namespace ShoppingCenter.Infrastructure.Notifications;

public class FirebaseSettings
{
    public const string SectionName = "Firebase";

    // Absolute or content-root-relative path to the Firebase service-account JSON
    // (Project Settings → Service accounts → Generate new private key).
    // Leave empty to run with notifications disabled (everything else still works).
    public string CredentialsPath { get; set; } = string.Empty;
}
