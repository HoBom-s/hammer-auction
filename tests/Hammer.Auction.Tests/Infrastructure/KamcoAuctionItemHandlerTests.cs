using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="KamcoAuctionItemHandler"/> upsert logic.
/// </summary>
public sealed class KamcoAuctionItemHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AuctionDbContext _db;
    private readonly KamcoAuctionItemHandler _handler;

    public KamcoAuctionItemHandlerTests()
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AuctionDbContext(options);
        _handler = new KamcoAuctionItemHandler(NullLogger<KamcoAuctionItemHandler>.Instance);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task HandleAsync_WithNewItems_ShouldInsertAsync()
    {
        List<string> messages =
        [
            SerializeMessage(1, 2, 3, "Item A"),
            SerializeMessage(4, 5, 6, "Item B"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.CltrNm == "Item A");
        items.Should().Contain(i => i.CltrNm == "Item B");
    }

    [Fact]
    public async Task HandleAsync_WithExistingItem_ShouldUpdateAsync()
    {
        KamcoAuctionItem existing = CreateEntity(1, 2, 3, "Old Name");
        _db.KamcoAuctionItems.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages = [SerializeMessage(1, 2, 3, "Updated Name")];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CltrNm.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithMixedInsertAndUpdate_ShouldHandleBothAsync()
    {
        KamcoAuctionItem existing = CreateEntity(1, 2, 3, "Existing");
        _db.KamcoAuctionItems.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages =
        [
            SerializeMessage(1, 2, 3, "Updated Existing"),
            SerializeMessage(10, 20, 30, "Brand New"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.CltrNm == "Updated Existing" && i.PlnmNo == 1);
        items.Should().Contain(i => i.CltrNm == "Brand New" && i.PlnmNo == 10);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateKeysInBatch_ShouldNotDuplicateInsertAsync()
    {
        List<string> messages =
        [
            SerializeMessage(1, 2, 3, "First"),
            SerializeMessage(1, 2, 3, "Second"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CltrNm.Should().Be("Second");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMessages_ShouldDoNothingAsync()
    {
        await _handler.HandleAsync([], _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ShouldSkipInvalidAsync()
    {
        List<string> messages =
        [
            "not valid json",
            SerializeMessage(1, 2, 3, "Valid"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<KamcoAuctionItem> items = await _db.KamcoAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CltrNm.Should().Be("Valid");
    }

    [Fact]
    public async Task HandleAsync_ShouldParseDatesCorrectlyAsync()
    {
        List<string> messages =
        [
            SerializeMessage(1, 2, 3, "Test", "20260315100000", "20260320170000"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        KamcoAuctionItem item = await _db.KamcoAuctionItems.SingleAsync();
        item.PbctBegnDtm.Offset.Should().Be(TimeSpan.FromHours(9));
        item.PbctBegnDtm.Day.Should().Be(15);
        item.PbctClsDtm.Day.Should().Be(20);
        item.PbctClsDtm.Hour.Should().Be(17);
    }

    [Fact]
    public void Topic_ShouldReturnCorrectTopic()
    {
        _handler.Topic.Should().Be("onbid-kamco-auction");
    }

    [Fact]
    public async Task HandleAsync_WithNullDb_ShouldThrowAsync()
    {
        Func<Task> act = () => _handler.HandleAsync([], null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static KamcoAuctionItem CreateEntity(
        long plnmNo,
        long pbctNo,
        long cltrNo,
        string cltrNm)
    {
        return KamcoAuctionItem.Create(
            plnmNo,
            pbctNo,
            cltrNo,
            cltrNm,
            "Category",
            "Addr1",
            "Addr2",
            100,
            200,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);
    }

    private static string SerializeMessage(
        long plnmNo,
        long pbctNo,
        long cltrNo,
        string cltrNm,
        string pbctBegnDtm = "20260101090000",
        string pbctClsDtm = "20260102090000")
    {
        return JsonSerializer.Serialize(
            new
            {
                plnmNo,
                pbctNo,
                cltrNo,
                cltrNm,
                ctgrFullNm = "Category",
                ldnmAdrs = "Address1",
                nmrdAdrs = "Address2",
                minBidPrc = 100L,
                apslAsesAvgAmt = 200L,
                bidMtdNm = "Method",
                pbctCltrStatNm = "Status",
                pbctBegnDtm,
                pbctClsDtm,
                uscbdCnt = 0,
                iqryCnt = 0,
                cltrImgFiles = (string?)null,
            },
            _jsonOptions);
    }
}
