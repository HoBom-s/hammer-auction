namespace Hammer.Auction.Application.Common;

/// <summary>
/// Kafka 알림 발송 요청 페이로드.
/// </summary>
public sealed record NotificationPayload(
    string TemplateKey,
    string RecipientToken,
    IReadOnlyDictionary<string, string> Variables);
