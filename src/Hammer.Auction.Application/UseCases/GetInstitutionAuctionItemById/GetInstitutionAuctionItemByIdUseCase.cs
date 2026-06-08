using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;

/// <summary>
/// Retrieves a single institution auction item by its surrogate ID.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetInstitutionAuctionItemByIdUseCase(IInstitutionAuctionItemRepository repository) : IGetInstitutionAuctionItemByIdUseCase
{
    /// <inheritdoc />
    public async Task<InstitutionAuctionItemResponse> ExecuteAsync(InstitutionAuctionItemId id, CancellationToken ct = default)
    {
        Domain.Entities.InstitutionAuctionItem item = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Institution auction item with ID {id} was not found.");

        return InstitutionAuctionItemResponse.FromEntity(item);
    }
}
