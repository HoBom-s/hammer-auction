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

    /// <summary>
    /// Gets the institution auction items table.
    /// </summary>
    public DbSet<InstitutionAuctionItem> InstitutionAuctionItems => Set<InstitutionAuctionItem>();

    /// <summary>
    /// Gets the Onbid code information table.
    /// </summary>
    public DbSet<OnbidCodeInfo> OnbidCodeInfos => Set<OnbidCodeInfo>();

    /// <summary>
    /// Gets the real estate trade records table.
    /// </summary>
    public DbSet<RealEstateTrade> RealEstateTrades => Set<RealEstateTrade>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);
    }
}
