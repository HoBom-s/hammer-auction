namespace Hammer.Auction.Application;

/// <summary>
///     아웃박스 워커 설정.
/// </summary>
public sealed class OutboxSettings
{
    /// <summary>Gets or sets the maximum number of messages to process per batch.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Gets or sets the retention period in days for processed messages.</summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>Gets or sets the maximum retry count before a message is skipped.</summary>
    public int MaxRetryCount { get; set; } = 5;
}
