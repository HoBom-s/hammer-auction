using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.Notifications;

/// <summary>
/// 알림 페이로드를 JSON 직렬화하여 아웃박스 메시지로 추가한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class NotificationSender(IOutboxWriter outboxWriter) : INotificationSender
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Send(NotificationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var message = OutboxMessage.Create(KafkaTopics.NotificationRequest, null, json);
        outboxWriter.Enqueue(message);
    }
}
