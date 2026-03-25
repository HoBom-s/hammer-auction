using Hammer.Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the auction service.
/// </summary>
public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Gets the KAMCO auction items table.
    /// </summary>
    public DbSet<KamcoAuctionItem> KamcoAuctionItems => Set<KamcoAuctionItem>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);
    }
}
