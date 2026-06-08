using Hammer.Auction.Infrastructure.Persistence;

namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
/// Handles a batch of Kafka messages for a specific topic.
/// </summary>
internal interface IKafkaMessageHandler
{
    /// <summary>
    /// Gets the Kafka topic this handler processes.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// Processes a batch of raw JSON messages.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleAsync(IReadOnlyList<string> messages, AuctionDbContext db, CancellationToken ct);
}
