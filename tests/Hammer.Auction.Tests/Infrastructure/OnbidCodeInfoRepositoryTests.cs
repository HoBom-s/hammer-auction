using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Integration tests for <see cref="OnbidCodeInfoRepository"/> using EF Core InMemory provider.
/// </summary>
[Collection("InMemory")]
public sealed class OnbidCodeInfoRepositoryTests(InMemoryFixture fixture) : IAsyncLifetime
{
    private static long _counter;

    private readonly List<long> _seededIds = [];

    private string _ctgrIdA = null!;
    private string _ctgrIdB = null!;
    private string _ctgrIdC = null!;

    public async Task InitializeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        _ctgrIdA = $"CTG-A-{Interlocked.Increment(ref _counter)}";
        _ctgrIdB = $"CTG-B-{Interlocked.Increment(ref _counter)}";
        _ctgrIdC = $"CTG-C-{Interlocked.Increment(ref _counter)}";

        var item1 = OnbidCodeInfo.Create(_ctgrIdA, "토지", "ROOT", "전체");
        var item2 = OnbidCodeInfo.Create(_ctgrIdB, "건물", "ROOT", "전체");
        var item3 = OnbidCodeInfo.Create(_ctgrIdC, "아파트", _ctgrIdA, "토지");

        db.OnbidCodeInfos.AddRange(item1, item2, item3);
        await db.SaveChangesAsync();

        _seededIds.AddRange([item1.Id, item2.Id, item3.Id]);
    }

    public async Task DisposeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        var toRemove = db.OnbidCodeInfos
            .Where(e => _seededIds.Contains(e.Id))
            .ToList();

        db.OnbidCodeInfos.RemoveRange(toRemove);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingItem_ShouldReturnEntityAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new OnbidCodeInfoRepository(db);

        OnbidCodeInfo? result = await repo.GetByIdAsync(_seededIds[0]);

        result.Should().NotBeNull();
        result!.CtgrId.Should().Be(_ctgrIdA);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNullAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new OnbidCodeInfoRepository(db);

        OnbidCodeInfo? result = await repo.GetByIdAsync(-1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResultsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new OnbidCodeInfoRepository(db);

        (IReadOnlyList<OnbidCodeInfo> items, var totalCount) =
            await repo.GetPagedAsync(1, 2, null);

        items.Should().HaveCount(2);
        totalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithParentIdFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new OnbidCodeInfoRepository(db);

        (IReadOnlyList<OnbidCodeInfo> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, _ctgrIdA);

        items.Should().AllSatisfy(item => item.CtgrHirkId.Should().Be(_ctgrIdA));
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderByCtgrIdAscendingAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new OnbidCodeInfoRepository(db);

        (IReadOnlyList<OnbidCodeInfo> items, var _) =
            await repo.GetPagedAsync(1, 100, null);

        items.Should().BeInAscendingOrder(i => i.CtgrId);
    }
}
