using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetAuctionItems;

/// <summary>
///     Retrieves a paginated list of KAMCO auction items.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetAuctionItemsUseCase(
    IKamcoAuctionItemRepository repository,
    IRealEstateTradeRepository tradeRepository) : IGetAuctionItemsUseCase
{
    /// <inheritdoc />
    public async Task<PagedResponse<KamcoAuctionItemResponse>> ExecuteAsync(
        GetAuctionItemsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        PageHelper.Validate(request.Page, request.Size);

        (IReadOnlyList<KamcoAuctionItem> items, var totalCount) = await repository.GetPagedAsync(
            request.Page,
            request.Size,
            request.Status,
            request.Category,
            request.Keyword,
            ct);

        IReadOnlyDictionary<string, RealEstateTrade> tradeByAddress = await FetchLatestTradesAsync(items, ct);

        var responses = items.Select(item =>
        {
            var response = KamcoAuctionItemResponse.FromEntity(item);

            if (tradeByAddress.TryGetValue(item.LdnmAdrs, out RealEstateTrade? trade))
                response = response with { LatestTradeAmount = trade.DealAmount, LatestTradeDate = $"{trade.DealYear:D4}-{trade.DealMonth:D2}" };

            return response;
        }).ToList();

        var totalPages = PageHelper.CalculateTotalPages(totalCount, request.Size);

        return new PagedResponse<KamcoAuctionItemResponse>(
            responses,
            request.Page,
            request.Size,
            totalCount,
            totalPages);
    }

    private async Task<IReadOnlyDictionary<string, RealEstateTrade>> FetchLatestTradesAsync(
        IReadOnlyList<KamcoAuctionItem> items,
        CancellationToken ct)
    {
        Dictionary<string, (string UmdNm, string Jibun)> addressToLocation =
            AddressParser.BuildLocationMap(items.Select(item => item.LdnmAdrs));

        if (addressToLocation.Count == 0)
            return new Dictionary<string, RealEstateTrade>();

        // Deduplicate locations to avoid redundant queries
        Dictionary<(string UmdNm, string Jibun), List<string>> locationToAddresses = AddressParser.Reverse(addressToLocation);

        // DbContext는 thread-safe하지 않으므로 순차 조회
        var dict = new Dictionary<string, RealEstateTrade>();

        foreach (((string UmdNm, string Jibun) location, List<string> addresses) in locationToAddresses)
        {
            IReadOnlyList<RealEstateTrade> trades = await tradeRepository.FindByLocationAsync(
                location.UmdNm, location.Jibun, 1, ct);

            if (trades.Count > 0)
            {
                foreach (var address in addresses)
                    dict[address] = trades[0];
            }
        }

        return dict;
    }
}
