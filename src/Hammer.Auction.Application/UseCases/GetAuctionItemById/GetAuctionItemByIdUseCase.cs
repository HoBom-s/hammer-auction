using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetAuctionItemById;

/// <summary>
/// Retrieves a single KAMCO auction item by its surrogate ID.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetAuctionItemByIdUseCase(IKamcoAuctionItemRepository repository) : IGetAuctionItemByIdUseCase
{
    /// <inheritdoc />
    public async Task<KamcoAuctionItemResponse> ExecuteAsync(long id, CancellationToken ct = default)
    {
        Domain.Entities.KamcoAuctionItem item = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Auction item with ID {id} was not found.");

        return KamcoAuctionItemResponse.FromEntity(item);
    }
}
