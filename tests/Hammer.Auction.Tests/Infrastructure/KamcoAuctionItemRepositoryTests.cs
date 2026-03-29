using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Integration tests for <see cref="KamcoAuctionItemRepository"/> using EF Core InMemory provider.
/// </summary>
[Collection("InMemory")]
public sealed class KamcoAuctionItemRepositoryTests(InMemoryFixture fixture) : IAsyncLifetime
{
    private static long _counter;

    private readonly List<long> _seededIds = [];

    public async Task InitializeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        KamcoAuctionItem item1 = CreateItem("강남 토지", "토지 / 대지", "입찰진행중", "서울 강남구 역삼동 123", "20260325100000");
        KamcoAuctionItem item2 = CreateItem("부산 아파트", "건물 / 아파트", "낙찰", "부산 금정구 부곡동 970", "20260320100000");
        KamcoAuctionItem item3 = CreateItem("서울 상가", "건물 / 상가", "입찰진행중", "서울 종로구 종로1가 1-5", "20260322100000");

        db.KamcoAuctionItems.AddRange(item1, item2, item3);
        await db.SaveChangesAsync();

        _seededIds.AddRange([item1.Id, item2.Id, item3.Id]);
    }

    public async Task DisposeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        var toRemove = db.KamcoAuctionItems
            .Where(e => _seededIds.Contains(e.Id))
            .ToList();

        db.KamcoAuctionItems.RemoveRange(toRemove);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingItem_ShouldReturnEntityAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        KamcoAuctionItem? result = await repo.GetByIdAsync(new KamcoAuctionItemId(_seededIds[0]));

        result.Should().NotBeNull();
        result!.CltrNm.Should().Be("강남 토지");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNullAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        KamcoAuctionItem? result = await repo.GetByIdAsync(new KamcoAuctionItemId(long.MaxValue));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResultsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        (IReadOnlyList<KamcoAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 2, null, null, null);

        items.Should().HaveCount(2);
        totalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithStatusFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        (IReadOnlyList<KamcoAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, "입찰진행중", null, null);

        items.Should().AllSatisfy(item => item.PbctCltrStatNm.Should().Be("입찰진행중"));
        totalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithCategoryFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        (IReadOnlyList<KamcoAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, null, "토지", null);

        items.Should().AllSatisfy(item => item.CtgrFullNm.Should().Contain("토지"));
        totalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithKeywordFilter_ShouldFilterByNameOrAddressAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        (IReadOnlyList<KamcoAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, null, null, "강남");

        items.Should().AllSatisfy(item =>
        {
            var matches = item.CltrNm.Contains("강남", StringComparison.Ordinal) ||
                          item.LdnmAdrs.Contains("강남", StringComparison.Ordinal) ||
                          item.NmrdAdrs.Contains("강남", StringComparison.Ordinal);
            matches.Should().BeTrue();
        });
        totalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderByPbctBegnDtmDescendingAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        (IReadOnlyList<KamcoAuctionItem> items, var _) =
            await repo.GetPagedAsync(1, 100, null, null, null);

        items.Should().BeInDescendingOrder(i => i.PbctBegnDtm);
    }

    [Fact]
    public async Task FindByNaturalKeysAsync_ShouldReturnMatchingItemsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        // Get the first seeded item to know its natural keys
        KamcoAuctionItem? seeded = await db.KamcoAuctionItems.FindAsync(_seededIds[0]);
        seeded.Should().NotBeNull();

        IReadOnlyList<(long PlnmNo, long PbctNo, long CltrNo)> keys =
            [(seeded!.PlnmNo, seeded.PbctNo, seeded.CltrNo)];

        IReadOnlyList<KamcoAuctionItem> result = await repo.FindByNaturalKeysAsync(keys);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(_seededIds[0]);
    }

    [Fact]
    public async Task Add_ShouldPersistNewItemAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new KamcoAuctionItemRepository(db);

        KamcoAuctionItem newItem = CreateItem("신규 물건", "토지", "입찰준비중", "경기도 수원시", "20260401100000");

        repo.Add(newItem);
        await db.SaveChangesAsync();

        _seededIds.Add(newItem.Id);

        KamcoAuctionItem? found = await repo.GetByIdAsync(new KamcoAuctionItemId(newItem.Id));
        found.Should().NotBeNull();
        found!.CltrNm.Should().Be("신규 물건");
    }

    private static KamcoAuctionItem CreateItem(
        string cltrNm,
        string ctgrFullNm,
        string status,
        string ldnmAdrs,
        string pbctBegnDtm)
    {
        var id = Interlocked.Increment(ref _counter);

        return KamcoAuctionItem.Create(
            id,
            id,
            id,
            cltrNm,
            ctgrFullNm,
            ldnmAdrs,
            "도로명주소",
            100,
            200,
            "일반경쟁",
            status,
            pbctBegnDtm,
            "20260327170000",
            0,
            0,
            null);
    }
}
