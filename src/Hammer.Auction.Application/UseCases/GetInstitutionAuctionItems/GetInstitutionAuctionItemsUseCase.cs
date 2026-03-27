using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;

/// <summary>
///     Retrieves a paginated list of institution auction items.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetInstitutionAuctionItemsUseCase(IInstitutionAuctionItemRepository repository) : IGetInstitutionAuctionItemsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<InstitutionAuctionItemResponse>> ExecuteAsync(
        GetInstitutionAuctionItemsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        PageHelper.Validate(request.Page, request.Size);

        (IReadOnlyList<InstitutionAuctionItem>, int) result = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Org,
            request.Category,
            request.Keyword,
            ct);

        (IReadOnlyList<InstitutionAuctionItem> institutionAuctionItems, var totalCount) = result;

        var responses = institutionAuctionItems.Select(InstitutionAuctionItemResponse.FromEntity).ToList();
        var totalPages = PageHelper.CalculateTotalPages(totalCount, request.Size);

        return new PagedResponse<InstitutionAuctionItemResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
