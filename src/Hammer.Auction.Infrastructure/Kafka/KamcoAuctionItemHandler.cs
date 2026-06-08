using System.Text.Json;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
///     Handles KAMCO auction item messages with upsert logic.
/// </summary>
internal sealed partial class KamcoAuctionItemHandler(
    ILogger<KamcoAuctionItemHandler> logger) : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public string Topic => KafkaTopics.KamcoAuction;

    /// <inheritdoc />
    public async Task HandleAsync(IReadOnlyList<string> messages, AuctionDbContext db, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        List<KamcoAuctionMessage> parsed = [];

        foreach (var json in messages)
        {
            try
            {
                KamcoAuctionMessage? msg = JsonSerializer.Deserialize<KamcoAuctionMessage>(json, _jsonOptions);

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
            .Select(m => (m.PlnmNo, m.PbctNo, m.CltrNo))
            .Distinct()
            .ToList();

        var plnmNos = keys.Select(k => k.PlnmNo).Distinct().ToList();

        List<KamcoAuctionItem> candidates = await db.KamcoAuctionItems
            .Where(e => plnmNos.Contains(e.PlnmNo))
            .ToListAsync(ct);

        HashSet<(long, long, long)> keySet = new(keys);

        Dictionary<(long, long, long), KamcoAuctionItem> existingMap = candidates
            .Where(e => keySet.Contains((e.PlnmNo, e.PbctNo, e.CltrNo)))
            .ToDictionary(e => (e.PlnmNo, e.PbctNo, e.CltrNo));

        var insertCount = 0;
        var updateCount = 0;

        foreach (KamcoAuctionMessage msg in parsed)
        {
            (long PlnmNo, long PbctNo, long CltrNo) key = (msg.PlnmNo, msg.PbctNo, msg.CltrNo);

            if (existingMap.TryGetValue(key, out KamcoAuctionItem? item))
            {
                item.UpdateFromSnapshot(
                    msg.CltrNm,
                    msg.CtgrFullNm,
                    msg.LdnmAdrs,
                    msg.NmrdAdrs,
                    msg.MinBidPrc,
                    msg.ApslAsesAvgAmt,
                    msg.BidMtdNm,
                    msg.PbctCltrStatNm,
                    msg.PbctBegnDtm,
                    msg.PbctClsDtm,
                    msg.UscbdCnt,
                    msg.IqryCnt,
                    SerializeImgFiles(msg.CltrImgFiles));

                updateCount++;
            }
            else
            {
                var newItem = KamcoAuctionItem.Create(
                    msg.PlnmNo,
                    msg.PbctNo,
                    msg.CltrNo,
                    msg.CltrNm,
                    msg.CtgrFullNm,
                    msg.LdnmAdrs,
                    msg.NmrdAdrs,
                    msg.MinBidPrc,
                    msg.ApslAsesAvgAmt,
                    msg.BidMtdNm,
                    msg.PbctCltrStatNm,
                    msg.PbctBegnDtm,
                    msg.PbctClsDtm,
                    msg.UscbdCnt,
                    msg.IqryCnt,
                    SerializeImgFiles(msg.CltrImgFiles));

                db.KamcoAuctionItems.Add(newItem);
                existingMap[key] = newItem;
                insertCount++;
            }
        }

        LogBatchProcessed(logger, insertCount, updateCount);
    }

    private static string? SerializeImgFiles(IReadOnlyList<string>? files) =>
        files is { Count: > 0 } ? JsonSerializer.Serialize(files) : null;

    [LoggerMessage(Level = LogLevel.Information, Message = "Processed batch: {Inserted} inserted, {Updated} updated")]
    private static partial void LogBatchProcessed(ILogger logger, int inserted, int updated);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize Kafka message: {Json}")]
    private static partial void LogDeserializationFailed(ILogger logger, string json, Exception ex);
}
