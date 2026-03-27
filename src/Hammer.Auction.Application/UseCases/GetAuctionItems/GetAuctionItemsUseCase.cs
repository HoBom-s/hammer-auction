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

        // Fetch latest trade per unique location in parallel
        List<(List<string> Addresses, Task<IReadOnlyList<RealEstateTrade>> Task)> queries = locationToAddresses
            .Select(kv => (kv.Value, tradeRepository.FindByLocationAsync(kv.Key.UmdNm, kv.Key.Jibun, 1, ct)))
            .ToList();

        await Task.WhenAll(queries.Select(q => q.Task));

        // Map back to addresses
        var dict = new Dictionary<string, RealEstateTrade>();

        foreach ((List<string> addresses, Task<IReadOnlyList<RealEstateTrade>> task) in queries)
        {
            IReadOnlyList<RealEstateTrade> trades = await task;

            if (trades.Count > 0)
            {
                foreach (var address in addresses)
                    dict[address] = trades[0];
            }
        }

        return dict;
    }
}
