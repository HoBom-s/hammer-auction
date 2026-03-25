using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetAuctionItems;

/// <summary>
/// Retrieves a paginated list of KAMCO auction items.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetAuctionItemsUseCase(IKamcoAuctionItemRepository repository) : IGetAuctionItemsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<KamcoAuctionItemResponse>> ExecuteAsync(
        GetAuctionItemsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Entities.KamcoAuctionItem>, int) result = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Status,
            request.Category,
            request.Keyword,
            ct);

        var responses = result.Item1.Select(KamcoAuctionItemResponse.FromEntity).ToList();
        var totalPages = (int)Math.Ceiling((double)result.Item2 / request.Size);

        return new PagedResponse<KamcoAuctionItemResponse>(
            responses,
            request.Page,
            request.Size,
            result.Item2,
            totalPages);
    }
}
