using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
///     Shared EF Core InMemory database fixture for repository integration tests.
/// </summary>
public sealed class InMemoryFixture
{
    private readonly string _databaseName = $"AuctionTestDb-{Guid.NewGuid()}";

    /// <summary>
    ///     Creates a new <see cref="AuctionDbContext" /> backed by the shared in-memory database.
    /// </summary>
    public AuctionDbContext CreateDbContext()
    {
        DbContextOptions<AuctionDbContext> options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AuctionDbContext(options);
    }
}

#pragma warning disable SA1402 // File may only contain a single type

/// <summary>
///     xUnit collection definition for in-memory integration tests.
/// </summary>
[CollectionDefinition("InMemory")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "xUnit collection convention")]
public sealed class InMemoryCollection : ICollectionFixture<InMemoryFixture>;
