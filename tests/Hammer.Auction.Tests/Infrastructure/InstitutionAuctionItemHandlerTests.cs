using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="InstitutionAuctionItemHandler"/> upsert logic.
/// </summary>
public sealed class InstitutionAuctionItemHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AuctionDbContext _db;
    private readonly InstitutionAuctionItemHandler _handler;

    public InstitutionAuctionItemHandlerTests()
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AuctionDbContext(options);
        _handler = new InstitutionAuctionItemHandler(NullLogger<InstitutionAuctionItemHandler>.Instance);
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
            SerializeMessage(1, 2, "Item A"),
            SerializeMessage(3, 4, "Item B"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.PlnmNm == "Item A");
        items.Should().Contain(i => i.PlnmNm == "Item B");
    }

    [Fact]
    public async Task HandleAsync_WithExistingItem_ShouldUpdateAsync()
    {
        InstitutionAuctionItem existing = CreateEntity(1, 2, "Old Name");
        _db.InstitutionAuctionItems.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages = [SerializeMessage(1, 2, "Updated Name")];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].PlnmNm.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithMixedInsertAndUpdate_ShouldHandleBothAsync()
    {
        InstitutionAuctionItem existing = CreateEntity(1, 2, "Existing");
        _db.InstitutionAuctionItems.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages =
        [
            SerializeMessage(1, 2, "Updated Existing"),
            SerializeMessage(10, 20, "Brand New"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.PlnmNm == "Updated Existing" && i.PlnmNo == 1);
        items.Should().Contain(i => i.PlnmNm == "Brand New" && i.PlnmNo == 10);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateKeysInBatch_ShouldNotDuplicateInsertAsync()
    {
        List<string> messages =
        [
            SerializeMessage(1, 2, "First"),
            SerializeMessage(1, 2, "Second"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].PlnmNm.Should().Be("Second");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMessages_ShouldDoNothingAsync()
    {
        await _handler.HandleAsync([], _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ShouldSkipInvalidAsync()
    {
        List<string> messages =
        [
            "not valid json",
            SerializeMessage(1, 2, "Valid"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<InstitutionAuctionItem> items = await _db.InstitutionAuctionItems.ToListAsync();
        items.Should().HaveCount(1);
        items[0].PlnmNm.Should().Be("Valid");
    }

    [Fact]
    public async Task HandleAsync_ShouldParseDatesCorrectlyAsync()
    {
        List<string> messages =
        [
            SerializeMessage(1, 2, "Test", "20260315100000", "20260320170000", "20260321100000"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        InstitutionAuctionItem item = await _db.InstitutionAuctionItems.SingleAsync();
        item.PbctBegnDtm.Offset.Should().Be(TimeSpan.Zero);
        item.PbctBegnDtm.Day.Should().Be(15);
        item.PbctClsDtm.Day.Should().Be(20);
        item.PbctClsDtm.Hour.Should().Be(8);
        item.PbctExctDtm.Day.Should().Be(21);
    }

    [Fact]
    public void Topic_ShouldReturnCorrectTopic()
    {
        _handler.Topic.Should().Be("onbid-institution-auction");
    }

    [Fact]
    public async Task HandleAsync_WithNullDb_ShouldThrowAsync()
    {
        Func<Task> act = () => _handler.HandleAsync([], null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static InstitutionAuctionItem CreateEntity(
        long plnmNo,
        long pbctNo,
        string plnmNm)
    {
        return InstitutionAuctionItem.Create(
            plnmNo,
            pbctNo,
            "CD01",
            "공고종류",
            "BD01",
            "입찰형태",
            plnmNm,
            "기관명",
            "20260101",
            "ORG-001",
            "MNMT-001",
            "BM01",
            "입찰방식",
            "TA01",
            "총액",
            "DP01",
            "처분방식",
            "PR01",
            "재산구분",
            "20260101090000",
            "20260102090000",
            "20260103090000",
            "CTG01",
            "카테고리");
    }

    private static string SerializeMessage(
        long plnmNo,
        long pbctNo,
        string plnmNm,
        string pbctBegnDtm = "20260101090000",
        string pbctClsDtm = "20260102090000",
        string pbctExctDtm = "20260103090000")
    {
        return JsonSerializer.Serialize(
            new
            {
                plnmNo,
                pbctNo,
                plnmKindCd = "CD01",
                plnmKindNm = "공고종류",
                bidDvsnCd = "BD01",
                bidDvsnNm = "입찰형태",
                plnmNm,
                orgNm = "기관명",
                plnmDt = "20260101",
                orgPlnmNo = "ORG-001",
                plnmMnmtNo = "MNMT-001",
                bidMtdCd = "BM01",
                bidMtdNm = "입찰방식",
                totAmtUnpcDvsnCd = "TA01",
                totAmtUnpcDvsnNm = "총액",
                dpslMtdCd = "DP01",
                dpslMtdNm = "처분방식",
                prptDvsnCd = "PR01",
                prptDvsnNm = "재산구분",
                pbctBegnDtm,
                pbctClsDtm,
                pbctExctDtm,
                ctgrId = "CTG01",
                ctgrFullNm = "카테고리",
            },
            _jsonOptions);
    }
}
