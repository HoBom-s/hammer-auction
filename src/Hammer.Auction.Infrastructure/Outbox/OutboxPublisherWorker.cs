using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Hammer.Auction.Infrastructure.Outbox;

/// <summary>
///     PostgreSQL LISTEN/NOTIFY로 아웃박스 메시지를 수신하고 Kafka로 발행하는 백그라운드 워커.
///     안전망으로 폴백 폴링(30초)을 병행한다.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed partial class OutboxPublisherWorker : BackgroundService
{
    private const string Channel = "outbox_ready";

    private static readonly TimeSpan _fallbackInterval = TimeSpan.FromSeconds(30);

    private readonly string _connectionString;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IOptions<OutboxSettings> _options;
    private readonly IProducer<string, string> _producer;
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<OutboxSettings> options,
        IConfiguration configuration,
        ILogger<OutboxPublisherWorker> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        _scopeFactory = scopeFactory;
        _producer = producer;
        _options = options;
        _logger = logger;
    }

    internal async Task<int> PublishPendingAsync(int batchSize, int maxRetryCount, CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AuctionDbContext db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        List<OutboxMessage> messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < maxRetryCount)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return 0;

        var published = 0;

        foreach (OutboxMessage message in messages)
        {
            try
            {
                Message<string, string> kafkaMessage = new() { Key = message.Key ?? string.Empty, Value = message.Payload };

                await _producer.ProduceAsync(message.Topic, kafkaMessage, ct);
                message.MarkAsProcessed();
                published++;
            }
#pragma warning disable CA1031
            catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
            {
                message.IncrementRetry();
                LogMessageFailed(_logger, message.Id, message.RetryCount, maxRetryCount, ex);
            }
        }

        await db.SaveChangesAsync(ct);
        return published;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        OutboxSettings settings = _options.Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(stoppingToken);

                await using NpgsqlCommand listenCmd = conn.CreateCommand();
                listenCmd.CommandText = $"LISTEN {Channel}";
                await listenCmd.ExecuteNonQueryAsync(stoppingToken);
                LogListening(_logger, Channel);

                await ListenLoopAsync(conn, settings, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogPublishFailed(_logger, ex);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox worker listening on channel '{Channel}'")]
    private static partial void LogListening(ILogger logger, string channel);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Outbox published {Count} messages")]
    private static partial void LogPublished(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox publish failed")]
    private static partial void LogPublishFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {MessageId} failed (retry {RetryCount}/{MaxRetryCount})")]
    private static partial void LogMessageFailed(ILogger logger, Guid messageId, int retryCount, int maxRetryCount, Exception ex);

    private async Task ListenLoopAsync(NpgsqlConnection conn, OutboxSettings settings, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // NOTIFY 수신 또는 30초 폴백 타임아웃
            using CancellationTokenSource timeoutCts = new(_fallbackInterval);

            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

            try
            {
                await conn.WaitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // 30초 타임아웃 - 폴백 폴링
            }

            var published = await PublishPendingAsync(settings.BatchSize, settings.MaxRetryCount, stoppingToken);

            if (published > 0)
                LogPublished(_logger, published);
        }
    }
}
