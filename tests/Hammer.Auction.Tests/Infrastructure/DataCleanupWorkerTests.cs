using System.Globalization;
using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Cleanup;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="DataCleanupWorker"/> cleanup predicates and scheduling logic.
/// Predicate tests use in-memory LINQ because the SQLite EF Core provider
/// cannot translate DateTimeOffset comparisons. Production queries against
/// PostgreSQL are covered by integration tests.
/// </summary>
public sealed class DataCleanupWorkerTests
{
    private const int KamcoRetentionDays = 90;
    private const int InstitutionRetentionDays = 90;
    private const int TradeRetentionDays = 1095;

    private static long _counter;

    [Fact]
    public void KamcoCleanupPredicate_ShouldMatchExpiredClosedItems()
    {
        List<KamcoAuctionItem> items =
        [
            CreateKamcoItem("낙찰", daysAgo: 100),
            CreateKamcoItem("인터넷입찰마감", daysAgo: 91),
        ];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-KamcoRetentionDays);
        var toDelete = items
            .Where(e => e.PbctCltrStatNm is "낙찰" or "인터넷입찰마감")
            .Where(e => e.UpdatedAt < cutoff)
            .ToList();

        toDelete.Should().HaveCount(2);
    }

    [Fact]
    public void KamcoCleanupPredicate_ShouldNotMatchActiveStatuses()
    {
        List<KamcoAuctionItem> items =
        [
            CreateKamcoItem("입찰준비중", daysAgo: 200),
            CreateKamcoItem("유찰", daysAgo: 200),
            CreateKamcoItem("수의계약가능", daysAgo: 200),
        ];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-KamcoRetentionDays);
        var toDelete = items
            .Where(e => e.PbctCltrStatNm is "낙찰" or "인터넷입찰마감")
            .Where(e => e.UpdatedAt < cutoff)
            .ToList();

        toDelete.Should().BeEmpty();
    }

    [Fact]
    public void KamcoCleanupPredicate_ShouldNotMatchRecentClosedItems()
    {
        List<KamcoAuctionItem> items = [CreateKamcoItem("낙찰", daysAgo: 30)];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-KamcoRetentionDays);
        var toDelete = items
            .Where(e => e.PbctCltrStatNm is "낙찰" or "인터넷입찰마감")
            .Where(e => e.UpdatedAt < cutoff)
            .ToList();

        toDelete.Should().BeEmpty();
    }

    [Fact]
    public void InstitutionCleanupPredicate_ShouldMatchExpiredItems()
    {
        List<InstitutionAuctionItem> items = [CreateInstitutionItem(daysAgo: 100)];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-InstitutionRetentionDays);
        var toDelete = items
            .Where(e => e.PbctClsDtm < cutoff)
            .ToList();

        toDelete.Should().HaveCount(1);
    }

    [Fact]
    public void InstitutionCleanupPredicate_ShouldNotMatchRecentItems()
    {
        List<InstitutionAuctionItem> items = [CreateInstitutionItem(daysAgo: 30)];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-InstitutionRetentionDays);
        var toDelete = items
            .Where(e => e.PbctClsDtm < cutoff)
            .ToList();

        toDelete.Should().BeEmpty();
    }

    [Fact]
    public void TradeCleanupPredicate_ShouldMatchExpiredItems()
    {
        List<RealEstateTrade> items = [CreateRealEstateTrade(daysAgo: 1100)];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-TradeRetentionDays);
        var toDelete = items
            .Where(e => e.CreatedAt < cutoff)
            .ToList();

        toDelete.Should().HaveCount(1);
    }

    [Fact]
    public void TradeCleanupPredicate_ShouldNotMatchRecentItems()
    {
        List<RealEstateTrade> items = [CreateRealEstateTrade(daysAgo: 365)];

        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-TradeRetentionDays);
        var toDelete = items
            .Where(e => e.CreatedAt < cutoff)
            .ToList();

        toDelete.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDelay_BeforeTargetHour_ShouldReturnSameDay()
    {
        DateTimeOffset now = new(2026, 3, 27, 10, 0, 0, TimeSpan.Zero);

        TimeSpan delay = DataCleanupWorker.CalculateDelayUntilNextRun(now, 18);

        delay.Should().Be(TimeSpan.FromHours(8));
    }

    [Fact]
    public void CalculateDelay_AfterTargetHour_ShouldReturnNextDay()
    {
        DateTimeOffset now = new(2026, 3, 27, 20, 0, 0, TimeSpan.Zero);

        TimeSpan delay = DataCleanupWorker.CalculateDelayUntilNextRun(now, 18);

        delay.Should().Be(TimeSpan.FromHours(22));
    }

    [Fact]
    public void CalculateDelay_ExactlyAtTargetHour_ShouldReturnNextDay()
    {
        DateTimeOffset now = new(2026, 3, 27, 18, 0, 0, TimeSpan.Zero);

        TimeSpan delay = DataCleanupWorker.CalculateDelayUntilNextRun(now, 18);

        delay.Should().Be(TimeSpan.FromHours(24));
    }

    private static long NextId() => Interlocked.Increment(ref _counter);

    private static KamcoAuctionItem CreateKamcoItem(string status, int daysAgo)
    {
        DateTimeOffset past = DateTimeOffset.UtcNow.AddDays(-daysAgo);
        var item = KamcoAuctionItem.Create(
            NextId(),
            1,
            1,
            "n",
            "c",
            "a1",
            "a2",
            100,
            200,
            "b",
            status,
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);

        typeof(KamcoAuctionItem).GetProperty(nameof(KamcoAuctionItem.UpdatedAt))!
            .SetValue(item, past);

        return item;
    }

    private static InstitutionAuctionItem CreateInstitutionItem(int daysAgo)
    {
        DateTimeOffset closeDtm = DateTimeOffset.UtcNow.AddDays(-daysAgo);
        var closeStr = closeDtm.ToOffset(TimeSpan.FromHours(9)).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        return InstitutionAuctionItem.Create(
            NextId(),
            1,
            "01",
            "공개경쟁",
            "01",
            "일반",
            "테스트",
            "테스트기관",
            "20260101",
            "ORG001",
            "MNG001",
            "01",
            "일반경쟁",
            "01",
            "총액",
            "01",
            "매각",
            "01",
            "토지",
            "20260101090000",
            closeStr,
            closeStr,
            "CTG01",
            "토지");
    }

    private static RealEstateTrade CreateRealEstateTrade(int daysAgo)
    {
        var item = RealEstateTrade.Create(
            "11110",
            1,
            "래미안",
            NextId().ToString(CultureInfo.InvariantCulture),
            "종로동",
            85000,
            2022,
            1,
            15,
            84.99m,
            10,
            2020);

        typeof(RealEstateTrade).GetProperty(nameof(RealEstateTrade.CreatedAt))!
            .SetValue(item, DateTimeOffset.UtcNow.AddDays(-daysAgo));

        return item;
    }
}
