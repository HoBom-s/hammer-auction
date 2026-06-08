using System.Text.Json;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
///     Handles real estate trade price messages with upsert logic.
/// </summary>
internal sealed partial class RealEstateTradeHandler(
    ILogger<RealEstateTradeHandler> logger) : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public string Topic => KafkaTopics.RealEstatePrice;

    /// <inheritdoc />
    public async Task HandleAsync(IReadOnlyList<string> messages, AuctionDbContext db, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        List<RealEstateTradeMessage> parsed = [];

        foreach (var json in messages)
        {
            try
            {
                RealEstateTradeMessage? msg = JsonSerializer.Deserialize<RealEstateTradeMessage>(json, _jsonOptions);

                if (msg is not null)
                    parsed.Add(msg);
            }
            catch (JsonException ex)
            {
                LogDeserializationFailed(logger, json, ex);
            }
        }

        if (parsed.Count == 0)
            return;

        var keys = parsed
            .Select(m => (m.LawdCd, m.PropertyType, m.Jibun, m.DealYear, m.DealMonth, m.DealDay, m.Area))
            .Distinct()
            .ToList();

        var lawdCds = keys.Select(k => k.LawdCd).Distinct().ToList();

        List<RealEstateTrade> candidates = await db.RealEstateTrades
            .Where(e => lawdCds.Contains(e.LawdCd))
            .ToListAsync(ct);

        HashSet<(string, int, string, int, int, int, decimal)> keySet = new(keys);

        Dictionary<(string, int, string, int, int, int, decimal), RealEstateTrade> existingMap = candidates
            .Where(e => keySet.Contains((e.LawdCd, e.PropertyType, e.Jibun, e.DealYear, e.DealMonth, e.DealDay, e.Area)))
            .ToDictionary(e => (e.LawdCd, e.PropertyType, e.Jibun, e.DealYear, e.DealMonth, e.DealDay, e.Area));

        var insertCount = 0;
        var updateCount = 0;

        foreach (RealEstateTradeMessage msg in parsed)
        {
            (string LawdCd, int PropertyType, string Jibun, int DealYear, int DealMonth, int DealDay, decimal Area) key =
                (msg.LawdCd, msg.PropertyType, msg.Jibun, msg.DealYear, msg.DealMonth, msg.DealDay, msg.Area);

            if (existingMap.TryGetValue(key, out RealEstateTrade? item))
            {
                item.UpdateFromSnapshot(
                    msg.BuildingName,
                    msg.UmdNm,
                    msg.DealAmount,
                    msg.Floor,
                    msg.BuildYear);

                updateCount++;
            }
            else
            {
                var newItem = RealEstateTrade.Create(
                    msg.LawdCd,
                    msg.PropertyType,
                    msg.BuildingName,
                    msg.Jibun,
                    msg.UmdNm,
                    msg.DealAmount,
                    msg.DealYear,
                    msg.DealMonth,
                    msg.DealDay,
                    msg.Area,
                    msg.Floor,
                    msg.BuildYear);

                db.RealEstateTrades.Add(newItem);
                existingMap[key] = newItem;
                insertCount++;
            }
        }

        LogBatchProcessed(logger, insertCount, updateCount);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processed real estate trade batch: {Inserted} inserted, {Updated} updated")]
    private static partial void LogBatchProcessed(ILogger logger, int inserted, int updated);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize real estate trade Kafka message: {Json}")]
    private static partial void LogDeserializationFailed(ILogger logger, string json, Exception ex);
}
