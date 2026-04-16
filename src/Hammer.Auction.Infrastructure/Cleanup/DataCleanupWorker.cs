using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hammer.Auction.Infrastructure.Cleanup;

/// <summary>
///     Background service that periodically deletes expired auction and trade data.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed partial class DataCleanupWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<CleanupSettings> options,
    IOptions<OutboxSettings> outboxOptions,
    ILogger<DataCleanupWorker> logger) : BackgroundService
{
    /// <summary>
    ///     Calculates the delay until the next cleanup run based on the configured UTC hour.
    /// </summary>
    internal static TimeSpan CalculateDelayUntilNextRun(DateTimeOffset now, int cleanupHourUtc)
    {
        DateTimeOffset today = new(now.UtcDateTime.Date, TimeSpan.Zero);
        DateTimeOffset nextRun = today.AddHours(cleanupHourUtc);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }

    /// <summary>
    ///     Deletes expired auction items and trade records based on retention settings.
    /// </summary>
    internal async Task RunCleanupAsync(CancellationToken ct)
    {
        CleanupSettings settings = options.Value;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        DateTimeOffset kamcoCutoff = now.AddDays(-settings.KamcoRetentionDays);
        DateTimeOffset institutionCutoff = now.AddDays(-settings.InstitutionRetentionDays);
        DateTimeOffset tradeCutoff = now.AddDays(-settings.RealEstateTradeRetentionDays);
        DateTimeOffset searchLogCutoff = now.AddDays(-settings.SearchLogRetentionDays);

        using IServiceScope scope = scopeFactory.CreateScope();
        AuctionDbContext db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

        var kamcoDeleted = await db.KamcoAuctionItems
            .Where(e => e.PbctCltrStatNm == "낙찰" || e.PbctCltrStatNm == "인터넷입찰마감")
            .Where(e => e.UpdatedAt < kamcoCutoff)
            .ExecuteDeleteAsync(ct);

        var institutionDeleted = await db.InstitutionAuctionItems
            .Where(e => e.PbctClsDtm < institutionCutoff)
            .ExecuteDeleteAsync(ct);

        var tradeDeleted = await db.RealEstateTrades
            .Where(e => e.CreatedAt < tradeCutoff)
            .ExecuteDeleteAsync(ct);

        OutboxSettings outboxSettings = outboxOptions.Value;
        DateTimeOffset outboxCutoff = now.AddDays(-outboxSettings.RetentionDays);

        var outboxDeleted = await db.OutboxMessages
            .Where(e => (e.ProcessedAt != null && e.ProcessedAt < outboxCutoff)
                || e.RetryCount >= outboxSettings.MaxRetryCount)
            .ExecuteDeleteAsync(ct);

        var searchLogDeleted = await db.SearchLogs
            .Where(e => e.SearchedAt < searchLogCutoff)
            .ExecuteDeleteAsync(ct);

        LogCleanupCompleted(logger, kamcoDeleted, institutionDeleted, tradeDeleted, outboxDeleted, searchLogDeleted);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = CalculateDelayUntilNextRun(DateTimeOffset.UtcNow, options.Value.CleanupHourUtc);
            LogNextRun(logger, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await RunCleanupAsync(stoppingToken);
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogCleanupFailed(logger, ex);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Data cleanup scheduled in {Delay}")]
    private static partial void LogNextRun(ILogger logger, TimeSpan delay);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Data cleanup completed: KAMCO={KamcoDeleted}, Institution={InstitutionDeleted}, RealEstateTrade={TradeDeleted}, Outbox={OutboxDeleted}, SearchLog={SearchLogDeleted}")]
    private static partial void LogCleanupCompleted(ILogger logger, int kamcoDeleted, int institutionDeleted, int tradeDeleted, int outboxDeleted, int searchLogDeleted);

    [LoggerMessage(Level = LogLevel.Error, Message = "Data cleanup failed")]
    private static partial void LogCleanupFailed(ILogger logger, Exception ex);
}
