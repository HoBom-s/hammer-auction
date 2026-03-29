using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;

/// <summary>
/// Use case contract for retrieving a single institution auction item by ID.
/// </summary>
public interface IGetInstitutionAuctionItemByIdUseCase
{
    /// <summary>
    /// Retrieves an institution auction item by its surrogate ID.
    /// </summary>
    public Task<InstitutionAuctionItemResponse> ExecuteAsync(InstitutionAuctionItemId id, CancellationToken ct = default);
}
