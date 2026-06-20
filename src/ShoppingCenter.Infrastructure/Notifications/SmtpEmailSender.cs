using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Infrastructure.Notifications;

// Sends customer order-confirmation emails over SMTP. Mirrors the Firebase push
// pattern: when SMTP isn't configured it no-ops, and every failure is logged and
// swallowed so it can never break order placement.
public class SmtpEmailSender : IEmailSender
{
    private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("en-US");

    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            // Not configured — nothing to do (order flow is unaffected out of the box).
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromAddress))
                return;

            // No address captured at checkout — can't send.
            if (string.IsNullOrWhiteSpace(order.CustomerEmail))
                return;

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = $"Order confirmation — {order.TotalAmount.ToString("C", Money)}",
                Body = BuildHtmlBody(order),
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            message.To.Add(new MailAddress(order.CustomerEmail, order.CustomerName));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            if (!string.IsNullOrEmpty(_settings.Username))
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

            // The order is already committed, so a client disconnect must NOT suppress the
            // confirmation: use a fresh timeout token rather than the request's. (SmtpClient.Timeout
            // only bounds the synchronous Send, not SendMailAsync, so we cap it ourselves.)
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
            await client.SendMailAsync(message, timeout.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order-confirmation email for order {OrderId}.", order.Id);
        }
    }

    private static string BuildHtmlBody(Order order)
    {
        var sb = new StringBuilder();
        sb.Append("<div style=\"font-family:Arial,Helvetica,sans-serif;color:#1b1a18;\">");
        sb.Append($"<h2 style=\"font-weight:600;\">Thanks for your order, {Encode(order.CustomerName)}!</h2>");
        sb.Append("<p>We've received your order and will be in touch soon. Here's a summary:</p>");

        sb.Append("<table cellpadding=\"8\" cellspacing=\"0\" style=\"border-collapse:collapse;width:100%;max-width:520px;\">");
        sb.Append("<thead><tr style=\"text-align:left;border-bottom:1px solid #ddd;\">"
            + "<th>Item</th><th style=\"text-align:center;\">Qty</th><th style=\"text-align:right;\">Price</th></tr></thead><tbody>");
        foreach (var item in order.Items)
        {
            sb.Append("<tr style=\"border-bottom:1px solid #eee;\">"
                + $"<td>{Encode(item.ProductName)}</td>"
                + $"<td style=\"text-align:center;\">{item.Quantity}</td>"
                + $"<td style=\"text-align:right;\">{item.LineTotal.ToString("C", Money)}</td></tr>");
        }
        sb.Append("</tbody><tfoot><tr><td colspan=\"2\" style=\"text-align:right;font-weight:600;\">Total</td>"
            + $"<td style=\"text-align:right;font-weight:600;\">{order.TotalAmount.ToString("C", Money)}</td></tr></tfoot>");
        sb.Append("</table>");

        sb.Append($"<p style=\"margin-top:16px;\"><strong>Shipping to:</strong><br>{Encode(order.ShippingAddress)}</p>");
        sb.Append("</div>");
        return sb.ToString();
    }

    // Minimal HTML escaping for the customer-supplied / product values interpolated above.
    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
