using FluentAssertions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests that the EF Core model applies cleanly to the EF Core InMemory provider.
/// </summary>
[Collection("InMemory")]
public sealed class AuctionDbContextSchemaTests(InMemoryFixture fixture)
{
    [Fact]
    public async Task Schema_ShouldApplyWithoutErrorsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        var canConnect = await db.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task KamcoAuctionItems_ShouldQueryWithoutErrorsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        List<KamcoAuctionItem> items = await db.KamcoAuctionItems.Take(1).ToListAsync();

        items.Should().NotBeNull();
    }

    [Fact]
    public async Task InstitutionAuctionItems_ShouldQueryWithoutErrorsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        List<InstitutionAuctionItem> items = await db.InstitutionAuctionItems.Take(1).ToListAsync();

        items.Should().NotBeNull();
    }

    [Fact]
    public async Task OnbidCodeInfos_ShouldQueryWithoutErrorsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        List<OnbidCodeInfo> items = await db.OnbidCodeInfos.Take(1).ToListAsync();

        items.Should().NotBeNull();
    }

    [Fact]
    public async Task RealEstateTrades_ShouldQueryWithoutErrorsAsync()
    {
        await using AuctionDbContext db = fixture.CreateDbContext();

        List<RealEstateTrade> items = await db.RealEstateTrades.Take(1).ToListAsync();

        items.Should().NotBeNull();
    }
}
