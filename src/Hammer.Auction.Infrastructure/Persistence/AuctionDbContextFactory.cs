using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hammer.Auction.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public sealed class AuctionDbContextFactory : IDesignTimeDbContextFactory<AuctionDbContext>
{
    /// <inheritdoc />
    public AuctionDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AuctionDbContext> optionsBuilder = new();

#pragma warning disable S2068 // Hard-coded credential for design-time migration only
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=hammer;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();
#pragma warning restore S2068

        return new AuctionDbContext(optionsBuilder.Options);
    }
}
