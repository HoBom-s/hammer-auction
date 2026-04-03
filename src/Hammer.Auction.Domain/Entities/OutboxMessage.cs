namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     트랜잭셔널 아웃박스 메시지.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; init; }

    public string Topic { get; private set; } = string.Empty;

    public string? Key { get; private set; }

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int RetryCount { get; private set; }

    public static OutboxMessage Create(string topic, string? key, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Key = key,
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void MarkAsProcessed() =>
        ProcessedAt ??= DateTimeOffset.UtcNow;

    public void IncrementRetry() =>
        RetryCount++;
}
