using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="RealEstateTradeHandler"/> upsert logic.
/// </summary>
public sealed class RealEstateTradeHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AuctionDbContext _db;
    private readonly RealEstateTradeHandler _handler;

    public RealEstateTradeHandlerTests()
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AuctionDbContext(options);
        _handler = new RealEstateTradeHandler(NullLogger<RealEstateTradeHandler>.Instance);
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
            SerializeMessage("11110", 1, "래미안", "123-4"),
            SerializeMessage("11110", 1, "자이", "456-7"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.BuildingName == "래미안");
        items.Should().Contain(i => i.BuildingName == "자이");
    }

    [Fact]
    public async Task HandleAsync_WithExistingItem_ShouldUpdateAsync()
    {
        RealEstateTrade existing = CreateEntity(buildingName: "Old Name");
        _db.RealEstateTrades.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages = [SerializeMessage("11110", 1, "Updated Name", "123-4")];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().HaveCount(1);
        items[0].BuildingName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithMixedInsertAndUpdate_ShouldHandleBothAsync()
    {
        RealEstateTrade existing = CreateEntity(buildingName: "Existing");
        _db.RealEstateTrades.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages =
        [
            SerializeMessage("11110", 1, "Updated Existing", "123-4"),
            SerializeMessage("11110", 1, "Brand New", "789-0"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.BuildingName == "Updated Existing" && i.Jibun == "123-4");
        items.Should().Contain(i => i.BuildingName == "Brand New" && i.Jibun == "789-0");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateKeysInBatch_ShouldNotDuplicateInsertAsync()
    {
        List<string> messages =
        [
            SerializeMessage("11110", 1, "First", "123-4"),
            SerializeMessage("11110", 1, "Second", "123-4"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().HaveCount(1);
        items[0].BuildingName.Should().Be("Second");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMessages_ShouldDoNothingAsync()
    {
        await _handler.HandleAsync([], _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ShouldSkipInvalidAsync()
    {
        List<string> messages =
        [
            "not valid json",
            SerializeMessage("11110", 1, "Valid", "123-4"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<RealEstateTrade> items = await _db.RealEstateTrades.ToListAsync();
        items.Should().HaveCount(1);
        items[0].BuildingName.Should().Be("Valid");
    }

    [Fact]
    public void Topic_ShouldReturnCorrectTopic()
    {
        _handler.Topic.Should().Be("real-estate-market-price");
    }

    [Fact]
    public async Task HandleAsync_WithNullDb_ShouldThrowAsync()
    {
        Func<Task> act = () => _handler.HandleAsync([], null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static RealEstateTrade CreateEntity(
        string lawdCd = "11110",
        int propertyType = 1,
        string? buildingName = "래미안",
        string jibun = "123-4",
        string umdNm = "종로동",
        long dealAmount = 85000,
        int dealYear = 2025,
        int dealMonth = 3,
        int dealDay = 15,
        decimal area = 84.99m,
        int? floor = 10,
        int? buildYear = 2020)
    {
        return RealEstateTrade.Create(
            lawdCd,
            propertyType,
            buildingName,
            jibun,
            umdNm,
            dealAmount,
            dealYear,
            dealMonth,
            dealDay,
            area,
            floor,
            buildYear);
    }

    private static string SerializeMessage(
        string lawdCd = "11110",
        int propertyType = 1,
        string? buildingName = "래미안",
        string jibun = "123-4",
        string umdNm = "종로동",
        long dealAmount = 85000,
        int dealYear = 2025,
        int dealMonth = 3,
        int dealDay = 15,
        decimal area = 84.99m,
        int? floor = 10,
        int? buildYear = 2020)
    {
        return JsonSerializer.Serialize(
            new
            {
                lawdCd,
                propertyType,
                buildingName,
                jibun,
                umdNm,
                dealAmount,
                dealYear,
                dealMonth,
                dealDay,
                area,
                floor,
                buildYear,
            },
            _jsonOptions);
    }
}
