using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;
using Hammer.Auction.Infrastructure.Persistence;
using Hammer.Auction.Infrastructure.Persistence.Repositories;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Integration tests for <see cref="InstitutionAuctionItemRepository"/> using EF Core InMemory provider.
/// </summary>
[Collection("InMemory")]
public sealed class InstitutionAuctionItemRepositoryTests(InMemoryFixture fixture) : IAsyncLifetime
{
    private static long _counter;

    private readonly List<long> _seededIds = [];

    public async Task InitializeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        InstitutionAuctionItem item1 = CreateItem("서울특별시", "토지 / 대지", "서울 공매", "20260325100000");
        InstitutionAuctionItem item2 = CreateItem("서울특별시", "건물 / 상가", "서울 건물매각", "20260320100000");
        InstitutionAuctionItem item3 = CreateItem("부산광역시", "토지 / 대지", "부산 공매", "20260322100000");

        db.InstitutionAuctionItems.AddRange(item1, item2, item3);
        await db.SaveChangesAsync();

        _seededIds.AddRange([item1.Id, item2.Id, item3.Id]);
    }

    public async Task DisposeAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        var toRemove = db.InstitutionAuctionItems
            .Where(e => _seededIds.Contains(e.Id))
            .ToList();

        db.InstitutionAuctionItems.RemoveRange(toRemove);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingItem_ShouldReturnEntityAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        InstitutionAuctionItem? result = await repo.GetByIdAsync(new InstitutionAuctionItemId(_seededIds[0]));

        result.Should().NotBeNull();
        result!.OrgNm.Should().Be("서울특별시");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNullAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        InstitutionAuctionItem? result = await repo.GetByIdAsync(new InstitutionAuctionItemId(long.MaxValue));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResultsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        (IReadOnlyList<InstitutionAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 2, null, null, null);

        items.Should().HaveCount(2);
        totalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithOrgFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        (IReadOnlyList<InstitutionAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, "부산광역시", null, null);

        items.Should().AllSatisfy(item => item.OrgNm.Should().Be("부산광역시"));
        totalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithCategoryFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        (IReadOnlyList<InstitutionAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, null, "토지", null);

        items.Should().AllSatisfy(item => item.CtgrFullNm.Should().Contain("토지"));
        totalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithKeywordFilter_ShouldFilterAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        (IReadOnlyList<InstitutionAuctionItem> items, var totalCount) =
            await repo.GetPagedAsync(1, 100, null, null, "부산");

        items.Should().AllSatisfy(item => item.PlnmNm.Should().Contain("부산"));
        totalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldOrderByPbctBegnDtmDescendingAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();
        var repo = new InstitutionAuctionItemRepository(db);

        (IReadOnlyList<InstitutionAuctionItem> items, var _) =
            await repo.GetPagedAsync(1, 100, null, null, null);

        items.Should().BeInDescendingOrder(i => i.PbctBegnDtm);
    }

    private static InstitutionAuctionItem CreateItem(
        string orgNm,
        string ctgrFullNm,
        string plnmNm,
        string pbctBegnDtm)
    {
        var id = Interlocked.Increment(ref _counter);

        return InstitutionAuctionItem.Create(
            id,
            id,
            "01",
            "공매공고",
            "02",
            "전자입찰",
            plnmNm,
            orgNm,
            "20260325",
            $"ORG-{id}",
            $"MNG-{id}",
            "03",
            "일반경쟁",
            "01",
            "총액",
            "01",
            "매각",
            "01",
            "토지",
            pbctBegnDtm,
            "20260327170000",
            "20260328100000",
            $"CTG-{id}",
            ctgrFullNm);
    }
}
