using System.Globalization;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Infrastructure.Notifications;

public class FcmPushSender : IPushSender
{
    // FCM caps a single multicast at 500 tokens.
    private const int MaxTokensPerMulticast = 500;

    private readonly FirebaseMessagingProvider _provider;
    private readonly IDeviceTokenRepository _tokens;
    private readonly ILogger<FcmPushSender> _logger;

    public FcmPushSender(
        FirebaseMessagingProvider provider,
        IDeviceTokenRepository tokens,
        ILogger<FcmPushSender> logger)
    {
        _provider = provider;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task NotifyOrderCreatedAsync(Order order, CancellationToken cancellationToken = default)
    {
        // The whole method is best-effort: any failure is logged and swallowed so it can never
        // break order placement.
        try
        {
            var messaging = _provider.Messaging;
            if (messaging is null)
                return; // Firebase not configured — nothing to do.

            var tokens = await _tokens.GetAllTokensAsync(cancellationToken);
            if (tokens.Count == 0)
                return;

            // Data-only message: the service worker / foreground handler builds and displays the
            // notification and owns the click → /admin/orders/:id redirect. Sending a `notification`
            // block as well would double-fire on web.
            var data = new Dictionary<string, string>
            {
                ["title"] = "New order received",
                ["body"] = $"{order.CustomerName} • {order.TotalAmount.ToString("C", CultureInfo.GetCultureInfo("en-US"))}",
                ["orderId"] = order.Id.ToString(),
                ["url"] = $"/admin/orders/{order.Id}"
            };

            var invalidTokens = new List<string>();

            foreach (var batch in Chunk(tokens, MaxTokensPerMulticast))
            {
                var message = new MulticastMessage
                {
                    Tokens = batch,
                    Data = data
                };

                var response = await messaging.SendEachForMulticastAsync(message, cancellationToken);

                // Collect tokens FCM says are dead so we can prune them.
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    var result = response.Responses[i];
                    if (result.IsSuccess)
                        continue;

                    var code = result.Exception?.MessagingErrorCode;
                    if (code is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                        invalidTokens.Add(batch[i]);
                }
            }

            if (invalidTokens.Count > 0)
            {
                await _tokens.RemoveRangeAsync(invalidTokens, cancellationToken);
                _logger.LogInformation("Pruned {Count} stale FCM token(s).", invalidTokens.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order-created notification for order {OrderId}.", order.Id);
        }
    }

    private static IEnumerable<List<string>> Chunk(IReadOnlyList<string> source, int size)
    {
        for (var i = 0; i < source.Count; i += size)
            yield return source.Skip(i).Take(size).ToList();
    }
}
