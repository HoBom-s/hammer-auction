using Confluent.Kafka;
using FluentAssertions;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Outbox;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hammer.Auction.Tests.Infrastructure;

public sealed class OutboxPublisherWorkerTests
{
    [Fact]
    public async Task PublishPendingAsync_WithMessages_ShouldPublishAndMarkProcessedAsync()
    {
        (OutboxPublisherWorker worker, IProducer<string, string> producer, AuctionDbContext db) = CreateWorker();

        var msg1 = OutboxMessage.Create("topic-a", "key-1", "payload-1");
        var msg2 = OutboxMessage.Create("topic-b", null, "payload-2");
        db.OutboxMessages.AddRange(msg1, msg2);
        await db.SaveChangesAsync();

        producer.ProduceAsync(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new DeliveryResult<string, string>());

        var published = await worker.PublishPendingAsync(100, 5, CancellationToken.None);

        published.Should().Be(2);
        await producer.Received(2).ProduceAsync(
            Arg.Any<string>(),
            Arg.Any<Message<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPendingAsync_WithNoMessages_ShouldReturnZeroAsync()
    {
        (OutboxPublisherWorker worker, IProducer<string, string> producer, AuctionDbContext _) = CreateWorker();

        var published = await worker.PublishPendingAsync(100, 5, CancellationToken.None);

        published.Should().Be(0);
        await producer.DidNotReceive().ProduceAsync(
            Arg.Any<string>(),
            Arg.Any<Message<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPendingAsync_WhenKafkaFails_ShouldIncrementRetryAsync()
    {
        (OutboxPublisherWorker worker, IProducer<string, string> producer, AuctionDbContext db) = CreateWorker();

        var message = OutboxMessage.Create("topic-a", "key-1", "payload-1");
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync();

        producer.ProduceAsync(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new ProduceException<string, string>(
                new Error(ErrorCode.BrokerNotAvailable),
                new DeliveryResult<string, string>()));

        var published = await worker.PublishPendingAsync(100, 5, CancellationToken.None);

        published.Should().Be(0);

        // Worker는 별도 scope에서 엔티티를 로드하므로 DB에서 재조회
        await db.Entry(message).ReloadAsync();
        message.RetryCount.Should().Be(1);
        message.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task PublishPendingAsync_ShouldSkipAlreadyProcessedMessagesAsync()
    {
        (OutboxPublisherWorker worker, IProducer<string, string> producer, AuctionDbContext db) = CreateWorker();

        var processed = OutboxMessage.Create("topic-a", "key-1", "payload-1");
        processed.MarkAsProcessed();
        var pending = OutboxMessage.Create("topic-b", "key-2", "payload-2");
        db.OutboxMessages.AddRange(processed, pending);
        await db.SaveChangesAsync();

        producer.ProduceAsync(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new DeliveryResult<string, string>());

        var published = await worker.PublishPendingAsync(100, 5, CancellationToken.None);

        published.Should().Be(1);
        await producer.Received(1).ProduceAsync(
            "topic-b",
            Arg.Any<Message<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishPendingAsync_ShouldSkipPoisonMessagesAsync()
    {
        (OutboxPublisherWorker worker, IProducer<string, string> producer, AuctionDbContext db) = CreateWorker();

        var poison = OutboxMessage.Create("topic-a", "key-1", "payload-1");
        for (var i = 0; i < 5; i++)
            poison.IncrementRetry();

        var healthy = OutboxMessage.Create("topic-b", "key-2", "payload-2");
        db.OutboxMessages.AddRange(poison, healthy);
        await db.SaveChangesAsync();

        producer.ProduceAsync(
                Arg.Any<string>(),
                Arg.Any<Message<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(new DeliveryResult<string, string>());

        var published = await worker.PublishPendingAsync(100, 5, CancellationToken.None);

        published.Should().Be(1);
        await producer.Received(1).ProduceAsync(
            "topic-b",
            Arg.Any<Message<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    private static (OutboxPublisherWorker Worker, IProducer<string, string> Producer, AuctionDbContext Db) CreateWorker()
    {
        var dbName = $"OutboxTest-{Guid.NewGuid()}";

        AuctionDbContext db = CreateDbContext(dbName);
        IProducer<string, string> producer = Substitute.For<IProducer<string, string>>();
        IOptions<OutboxSettings> settings = Options.Create(new OutboxSettings { BatchSize = 100 });

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost",
            })
            .Build();

        ServiceCollection services = new();
        services.AddScoped(_ => CreateDbContext(dbName));
        ServiceProvider sp = services.BuildServiceProvider();

        IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        OutboxPublisherWorker worker = new(
            scopeFactory,
            producer,
            settings,
            configuration,
            NullLogger<OutboxPublisherWorker>.Instance);

        return (worker, producer, db);
    }

    private static AuctionDbContext CreateDbContext(string dbName)
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AuctionDbContext(options);
    }
}
