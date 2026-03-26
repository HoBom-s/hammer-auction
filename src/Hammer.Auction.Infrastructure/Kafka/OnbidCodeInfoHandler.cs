using System.Text.Json;
using Hammer.Auction.Application;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Kafka;

/// <summary>
///     Handles Onbid code information messages with upsert logic.
/// </summary>
internal sealed partial class OnbidCodeInfoHandler(
    ILogger<OnbidCodeInfoHandler> logger) : IKafkaMessageHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public string Topic => KafkaTopics.CodeInfo;

    /// <inheritdoc />
    public async Task HandleAsync(IReadOnlyList<string> messages, AuctionDbContext db, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);

        List<OnbidCodeInfoMessage> parsed = [];

        foreach (var json in messages)
        {
            try
            {
                OnbidCodeInfoMessage? msg = JsonSerializer.Deserialize<OnbidCodeInfoMessage>(json, _jsonOptions);

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

        var ctgrIds = parsed.Select(m => m.CtgrId).Distinct().ToList();

        List<OnbidCodeInfo> candidates = await db.OnbidCodeInfos
            .Where(e => ctgrIds.Contains(e.CtgrId))
            .ToListAsync(ct);

        var existingMap = candidates.ToDictionary(e => e.CtgrId);

        var insertCount = 0;
        var updateCount = 0;

        foreach (OnbidCodeInfoMessage msg in parsed)
        {
            if (existingMap.TryGetValue(msg.CtgrId, out OnbidCodeInfo? item))
            {
                item.UpdateFromSnapshot(
                    msg.CtgrNm,
                    msg.CtgrHirkId,
                    msg.CtgrHirkNm);

                updateCount++;
            }
            else
            {
                var newItem = OnbidCodeInfo.Create(
                    msg.CtgrId,
                    msg.CtgrNm,
                    msg.CtgrHirkId,
                    msg.CtgrHirkNm);

                db.OnbidCodeInfos.Add(newItem);
                existingMap[msg.CtgrId] = newItem;
                insertCount++;
            }
        }

        LogBatchProcessed(logger, insertCount, updateCount);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Processed code info batch: {Inserted} inserted, {Updated} updated")]
    private static partial void LogBatchProcessed(ILogger logger, int inserted, int updated);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize code info Kafka message: {Json}")]
    private static partial void LogDeserializationFailed(ILogger logger, string json, Exception ex);
}
