namespace ShoppingCenter.Infrastructure.Notifications;

public class EmailSettings
{
    public const string SectionName = "Email";

    // SMTP server host (e.g. smtp.gmail.com). Leave empty to run with customer
    // email disabled — order placement and everything else still works.
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    // Use STARTTLS/SSL for the connection (true for port 587/465 on most providers).
    public bool UseSsl { get; set; } = true;

    // SMTP auth. Leave both empty for an unauthenticated relay.
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // The address the confirmation is sent "from". Also required for email to be
    // considered configured — leave empty to keep customer email disabled.
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Shopping Center";

    // Hard cap on how long a single send may take, so a stalled mail server can
    // never hang the public checkout response.
    public int TimeoutSeconds { get; set; } = 10;
}
