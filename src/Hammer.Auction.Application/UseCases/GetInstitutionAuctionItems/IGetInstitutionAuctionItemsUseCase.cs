using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;

/// <summary>
/// Use case contract for retrieving paginated institution auction items.
/// </summary>
public interface IGetInstitutionAuctionItemsUseCase
{
    /// <summary>
    /// Retrieves a paginated list of institution auction items.
    /// </summary>
    public Task<PagedResponse<InstitutionAuctionItemResponse>> ExecuteAsync(
        GetInstitutionAuctionItemsRequest request,
        CancellationToken ct = default);
}
