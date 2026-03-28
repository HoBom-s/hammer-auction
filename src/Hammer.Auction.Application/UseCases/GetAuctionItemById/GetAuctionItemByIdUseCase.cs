using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Analysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetAuctionItemById;

/// <summary>
/// Retrieves a single KAMCO auction item by its surrogate ID, enriched with nearby real estate trades.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetAuctionItemByIdUseCase(
    IKamcoAuctionItemRepository repository,
    IRealEstateTradeRepository tradeRepository) : IGetAuctionItemByIdUseCase
{
    private const int RecentTradeLimit = 20;

    /// <inheritdoc />
    public async Task<KamcoAuctionItemResponse> ExecuteAsync(long id, CancellationToken ct = default)
    {
        KamcoAuctionItem item = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Auction item with ID {id} was not found.");

        IReadOnlyList<RealEstateTradeResponse> trades = await FetchRecentTradesAsync(item.LdnmAdrs, ct);

        InvestmentAnalysis? analysis = InvestmentAnalyzer.Analyze(
            item.MinBidPrc,
            item.UscbdCnt,
            item.IqryCnt,
            item.CtgrFullNm,
            trades,
            DateOnly.FromDateTime(DateTime.UtcNow),
            item.ApslAsesAvgAmt);

        return KamcoAuctionItemResponse.FromEntity(item) with
        {
            RecentTrades = trades,
            InvestmentAnalysis = analysis,
        };
    }

    private async Task<IReadOnlyList<RealEstateTradeResponse>> FetchRecentTradesAsync(
        string ldnmAdrs,
        CancellationToken ct)
    {
        (string? UmdNm, string? Jibun) parsed = AddressParser.ParseLocation(ldnmAdrs);

        if (parsed.UmdNm is null || parsed.Jibun is null)
            return [];

        IReadOnlyList<RealEstateTrade> trades = await tradeRepository.FindByLocationAsync(
            parsed.UmdNm,
            parsed.Jibun,
            RecentTradeLimit,
            ct);

        return trades.Select(RealEstateTradeResponse.FromEntity).ToList();
    }
}
