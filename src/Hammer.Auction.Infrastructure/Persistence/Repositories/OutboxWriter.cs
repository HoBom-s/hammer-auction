using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

[ExcludeFromCodeCoverage]
internal sealed class OutboxWriter(AuctionDbContext db) : IOutboxWriter
{
    public void Enqueue(OutboxMessage message) => db.OutboxMessages.Add(message);
}
