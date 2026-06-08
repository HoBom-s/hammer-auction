using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Application;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Notifications;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

public sealed class NotificationSenderTests
{
    private readonly IOutboxWriter _outboxWriter = Substitute.For<IOutboxWriter>();
    private readonly NotificationSender _sut;

    public NotificationSenderTests()
    {
        _sut = new NotificationSender(_outboxWriter);
    }

    [Fact]
    public void Send_ShouldEnqueueOutboxMessageWithCorrectTopicAndPayload()
    {
        NotificationPayload payload = new(
            "bid-opened",
            "ExponentPushToken[abc123]",
            new Dictionary<string, string> { ["itemName"] = "아파트" });

        _sut.Send(payload);

        _outboxWriter.Received(1).Enqueue(Arg.Is<OutboxMessage>(m =>
            m.Topic == KafkaTopics.NotificationRequest
            && m.Key == null));

        var captured = (OutboxMessage)_outboxWriter.ReceivedCalls().Single().GetArguments()[0]!;
        using var doc = JsonDocument.Parse(captured.Payload);
        doc.RootElement.GetProperty("templateKey").GetString().Should().Be("bid-opened");
        doc.RootElement.GetProperty("recipientToken").GetString().Should().Be("ExponentPushToken[abc123]");
        doc.RootElement.GetProperty("variables").GetProperty("itemName").GetString().Should().Be("아파트");
    }

    [Fact]
    public void Send_WithEmptyVariables_ShouldSerializeEmptyObject()
    {
        NotificationPayload payload = new("welcome", "ExponentPushToken[xyz]", new Dictionary<string, string>());

        _sut.Send(payload);

        var captured = (OutboxMessage)_outboxWriter.ReceivedCalls().Single().GetArguments()[0]!;
        using var doc = JsonDocument.Parse(captured.Payload);
        doc.RootElement.GetProperty("templateKey").GetString().Should().Be("welcome");
        doc.RootElement.GetProperty("recipientToken").GetString().Should().Be("ExponentPushToken[xyz]");
        doc.RootElement.GetProperty("variables").EnumerateObject().Should().BeEmpty();
    }

    [Fact]
    public void Send_WithNullPayload_ShouldThrowArgumentNullException()
    {
        Action act = () => _sut.Send(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
