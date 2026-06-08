using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Integration tests for <see cref="RealEstateTradeRepository"/> using EF Core InMemory provider.
/// </summary>
[Collection("InMemory")]
public sealed class RealEstateTradeRepositoryTests(InMemoryFixture fixture) : IAsyncLifetime
{
    private static long _counter;

    private readonly List<long> _seededIds = [];

    public async Task InitializeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        RealEstateTrade trade1 = CreateTrade("11110", "종로동", "123-4", 85000, 2026, 3, 15);
        RealEstateTrade trade2 = CreateTrade("11110", "종로동", "123-4", 82000, 2025, 12, 20);
        RealEstateTrade trade3 = CreateTrade("11110", "종로동", "123-4", 78000, 2025, 6, 10);
        RealEstateTrade trade4 = CreateTrade("11110", "사직동", "456-7", 120000, 2026, 1, 5);

        db.RealEstateTrades.AddRange(trade1, trade2, trade3, trade4);
        await db.SaveChangesAsync();

        _seededIds.AddRange([trade1.Id, trade2.Id, trade3.Id, trade4.Id]);
    }

    public async Task DisposeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        var toRemove = db.RealEstateTrades
            .Where(e => _seededIds.Contains(e.Id))
            .ToList();

        db.RealEstateTrades.RemoveRange(toRemove);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task FindByLocationAsync_WithMatchingLocation_ShouldReturnTradesAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new RealEstateTradeRepository(db);

        IReadOnlyList<RealEstateTrade> result =
            await repo.FindByLocationAsync("종로동", "123-4", 10);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindByLocationAsync_WithNoMatch_ShouldReturnEmptyAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new RealEstateTradeRepository(db);

        IReadOnlyList<RealEstateTrade> result =
            await repo.FindByLocationAsync("없는동", "999-9", 10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindByLocationAsync_WithLimit_ShouldRespectLimitAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new RealEstateTradeRepository(db);

        IReadOnlyList<RealEstateTrade> result =
            await repo.FindByLocationAsync("종로동", "123-4", 1);

        result.Should().HaveCount(1);
        result[0].DealAmount.Should().Be(85000); // Most recent (2026-03-15)
    }

    [Fact]
    public async Task FindByLocationAsync_ShouldOrderByDateDescendingAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new RealEstateTradeRepository(db);

        IReadOnlyList<RealEstateTrade> result =
            await repo.FindByLocationAsync("종로동", "123-4", 10);

        var dateKeys = result.Select(t => (t.DealYear * 10000) + (t.DealMonth * 100) + t.DealDay).ToList();
        dateKeys.Should().BeInDescendingOrder();
    }

    private static RealEstateTrade CreateTrade(
        string lawdCd,
        string umdNm,
        string jibun,
        long dealAmount,
        int dealYear,
        int dealMonth,
        int dealDay)
    {
        var id = Interlocked.Increment(ref _counter);

        return RealEstateTrade.Create(
            lawdCd,
            1,
            $"Building-{id}",
            jibun,
            umdNm,
            dealAmount,
            dealYear,
            dealMonth,
            dealDay,
            84.99m + id,
            10,
            2020);
    }
}
