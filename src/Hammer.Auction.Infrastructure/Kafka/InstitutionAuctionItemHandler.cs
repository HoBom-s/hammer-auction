using System.Text.Json;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
///     Handles institution auction item messages with upsert logic.
/// </summary>
internal sealed partial class InstitutionAuctionItemHandler(
    ILogger<InstitutionAuctionItemHandler> logger) : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public string Topic => KafkaTopics.InstitutionAuction;

    /// <inheritdoc />
    public async Task HandleAsync(IReadOnlyList<string> messages, AuctionDbContext db, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        List<InstitutionAuctionMessage> parsed = [];

        foreach (var json in messages)
        {
            try
            {
                InstitutionAuctionMessage? msg = JsonSerializer.Deserialize<InstitutionAuctionMessage>(json, _jsonOptions);

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
            .Select(m => (m.PlnmNo, m.PbctNo))
            .Distinct()
            .ToList();

        var plnmNos = keys.Select(k => k.PlnmNo).Distinct().ToList();

        List<InstitutionAuctionItem> candidates = await db.InstitutionAuctionItems
            .Where(e => plnmNos.Contains(e.PlnmNo))
            .ToListAsync(ct);

        HashSet<(long, long)> keySet = new(keys);

        Dictionary<(long, long), InstitutionAuctionItem> existingMap = candidates
            .Where(e => keySet.Contains((e.PlnmNo, e.PbctNo)))
            .ToDictionary(e => (e.PlnmNo, e.PbctNo));

        var insertCount = 0;
        var updateCount = 0;

        foreach (InstitutionAuctionMessage msg in parsed)
        {
            (long PlnmNo, long PbctNo) key = (msg.PlnmNo, msg.PbctNo);

            if (existingMap.TryGetValue(key, out InstitutionAuctionItem? item))
            {
                item.UpdateFromSnapshot(
                    msg.PlnmKindCd,
                    msg.PlnmKindNm,
                    msg.BidDvsnCd,
                    msg.BidDvsnNm,
                    msg.PlnmNm,
                    msg.OrgNm,
                    msg.PlnmDt,
                    msg.OrgPlnmNo,
                    msg.PlnmMnmtNo,
                    msg.BidMtdCd,
                    msg.BidMtdNm,
                    msg.TotAmtUnpcDvsnCd,
                    msg.TotAmtUnpcDvsnNm,
                    msg.DpslMtdCd,
                    msg.DpslMtdNm,
                    msg.PrptDvsnCd,
                    msg.PrptDvsnNm,
                    msg.PbctBegnDtm,
                    msg.PbctClsDtm,
                    msg.PbctExctDtm,
                    msg.CtgrId,
                    msg.CtgrFullNm);

                updateCount++;
            }
            else
            {
                var newItem = InstitutionAuctionItem.Create(
                    msg.PlnmNo,
                    msg.PbctNo,
                    msg.PlnmKindCd,
                    msg.PlnmKindNm,
                    msg.BidDvsnCd,
                    msg.BidDvsnNm,
                    msg.PlnmNm,
                    msg.OrgNm,
                    msg.PlnmDt,
                    msg.OrgPlnmNo,
                    msg.PlnmMnmtNo,
                    msg.BidMtdCd,
                    msg.BidMtdNm,
                    msg.TotAmtUnpcDvsnCd,
                    msg.TotAmtUnpcDvsnNm,
                    msg.DpslMtdCd,
                    msg.DpslMtdNm,
                    msg.PrptDvsnCd,
                    msg.PrptDvsnNm,
                    msg.PbctBegnDtm,
                    msg.PbctClsDtm,
                    msg.PbctExctDtm,
                    msg.CtgrId,
                    msg.CtgrFullNm);

                db.InstitutionAuctionItems.Add(newItem);
                existingMap[key] = newItem;
                insertCount++;
            }
        }

        LogBatchProcessed(logger, insertCount, updateCount);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processed institution batch: {Inserted} inserted, {Updated} updated")]
    private static partial void LogBatchProcessed(ILogger logger, int inserted, int updated);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize institution Kafka message: {Json}")]
    private static partial void LogDeserializationFailed(ILogger logger, string json, Exception ex);
}
