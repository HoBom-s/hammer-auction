using FluentAssertions;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Domain;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var message = OutboxMessage.Create("test-topic", "key-1", """{"data":1}""");

        message.Id.Should().NotBeEmpty();
        message.Topic.Should().Be("test-topic");
        message.Key.Should().Be("key-1");
        message.Payload.Should().Be("""{"data":1}""");
        message.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        message.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullKey_ShouldAllowNullKey()
    {
        var message = OutboxMessage.Create("topic", null, "payload");

        message.Key.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetProcessedAt()
    {
        var message = OutboxMessage.Create("topic", null, "payload");

        message.MarkAsProcessed();

        message.ProcessedAt.Should().NotBeNull();
        message.ProcessedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsProcessed_CalledTwice_ShouldBeIdempotent()
    {
        var message = OutboxMessage.Create("topic", null, "payload");

        message.MarkAsProcessed();
        DateTimeOffset? firstProcessedAt = message.ProcessedAt;

        message.MarkAsProcessed();

        message.ProcessedAt.Should().Be(firstProcessedAt);
    }

    [Fact]
    public void Create_ShouldInitializeRetryCountToZero()
    {
        var message = OutboxMessage.Create("topic", null, "payload");

        message.RetryCount.Should().Be(0);
    }

    [Fact]
    public void IncrementRetry_ShouldIncreaseRetryCount()
    {
        var message = OutboxMessage.Create("topic", null, "payload");

        message.IncrementRetry();
        message.IncrementRetry();

        message.RetryCount.Should().Be(2);
    }
}
