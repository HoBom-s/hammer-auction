using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Kafka;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="OnbidCodeInfoHandler"/> upsert logic.
/// </summary>
public sealed class OnbidCodeInfoHandlerTests : IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly AuctionDbContext _db;
    private readonly OnbidCodeInfoHandler _handler;

    public OnbidCodeInfoHandlerTests()
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AuctionDbContext(options);
        _handler = new OnbidCodeInfoHandler(NullLogger<OnbidCodeInfoHandler>.Instance);
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
            SerializeMessage("CTG01", "토지"),
            SerializeMessage("CTG02", "건물"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.CtgrNm == "토지");
        items.Should().Contain(i => i.CtgrNm == "건물");
    }

    [Fact]
    public async Task HandleAsync_WithExistingItem_ShouldUpdateAsync()
    {
        var existing = OnbidCodeInfo.Create("CTG01", "Old Name", "ROOT", "Root");
        _db.OnbidCodeInfos.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages = [SerializeMessage("CTG01", "Updated Name")];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CtgrNm.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithMixedInsertAndUpdate_ShouldHandleBothAsync()
    {
        var existing = OnbidCodeInfo.Create("CTG01", "Existing", "ROOT", "Root");
        _db.OnbidCodeInfos.Add(existing);
        await _db.SaveChangesAsync();

        List<string> messages =
        [
            SerializeMessage("CTG01", "Updated Existing"),
            SerializeMessage("CTG02", "Brand New"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.CtgrNm == "Updated Existing" && i.CtgrId == "CTG01");
        items.Should().Contain(i => i.CtgrNm == "Brand New" && i.CtgrId == "CTG02");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateKeysInBatch_ShouldNotDuplicateInsertAsync()
    {
        List<string> messages =
        [
            SerializeMessage("CTG01", "First"),
            SerializeMessage("CTG01", "Second"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CtgrNm.Should().Be("Second");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMessages_ShouldDoNothingAsync()
    {
        await _handler.HandleAsync([], _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ShouldSkipInvalidAsync()
    {
        List<string> messages =
        [
            "not valid json",
            SerializeMessage("CTG01", "Valid"),
        ];

        await _handler.HandleAsync(messages, _db, CancellationToken.None);
        await _db.SaveChangesAsync();

        List<OnbidCodeInfo> items = await _db.OnbidCodeInfos.ToListAsync();
        items.Should().HaveCount(1);
        items[0].CtgrNm.Should().Be("Valid");
    }

    [Fact]
    public void Topic_ShouldReturnCorrectTopic()
    {
        _handler.Topic.Should().Be("onbid-code-info");
    }

    [Fact]
    public async Task HandleAsync_WithNullDb_ShouldThrowAsync()
    {
        Func<Task> act = () => _handler.HandleAsync([], null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static string SerializeMessage(
        string ctgrId,
        string ctgrNm,
        string ctgrHirkId = "ROOT",
        string ctgrHirkNm = "Root")
    {
        return JsonSerializer.Serialize(
            new
            {
                ctgrId,
                ctgrNm,
                ctgrHirkId,
                ctgrHirkNm,
            },
            _jsonOptions);
    }
}
