using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetAuctionItemByIdUseCase"/>.
/// </summary>
public sealed class GetAuctionItemByIdUseCaseTests
{
    private readonly IKamcoAuctionItemRepository _repository = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly IRealEstateTradeRepository _tradeRepository = Substitute.For<IRealEstateTradeRepository>();
    private readonly GetAuctionItemByIdUseCase _sut;

    public GetAuctionItemByIdUseCaseTests()
    {
        _sut = new GetAuctionItemByIdUseCase(_repository, _tradeRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingItem_ShouldReturnResponseAsync()
    {
        KamcoAuctionItem entity = CreateEntity(1, 100, 200, 300, "Test Item");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);
        _tradeRepository.FindByLocationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RealEstateTrade>());

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.Should().NotBeNull();
        result.PlnmNo.Should().Be(100);
        result.PbctNo.Should().Be(200);
        result.CltrNo.Should().Be(300);
        result.CltrNm.Should().Be("Test Item");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentItem_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetByIdAsync(999L, Arg.Any<CancellationToken>()).Returns((KamcoAuctionItem?)null);

        Func<Task> act = () => _sut.ExecuteAsync(999L);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapDiscountRateCorrectlyAsync()
    {
        KamcoAuctionItem entity = CreateEntity(1, 100, 200, 300, "Item", minBidPrc: 80, apslAsesAvgAmt: 100);
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);
        _tradeRepository.FindByLocationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RealEstateTrade>());

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.DiscountRate.Should().Be(20.0);
    }

    [Fact]
    public async Task ExecuteAsync_WithMatchingTrades_ShouldIncludeRecentTradesAsync()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "부곡 롯데캐슬",
            ldnmAdrs: "부산광역시 금정구 부곡동 970 롯데캐슬디아망 제103동 제9층 제901호");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        List<RealEstateTrade> trades =
        [
            CreateTrade("26260", "부곡동", "970", 85000, 2026, 1, 15),
            CreateTrade("26260", "부곡동", "970", 82000, 2025, 12, 20),
        ];
        _tradeRepository.FindByLocationAsync("부곡동", "970", 20, Arg.Any<CancellationToken>())
            .Returns(trades);

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.RecentTrades.Should().NotBeNull();
        result.RecentTrades.Should().HaveCount(2);
        result.RecentTrades![0].DealAmount.Should().Be(85000);
        result.RecentTrades[1].DealAmount.Should().Be(82000);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnparsableAddress_ShouldReturnEmptyTradesAsync()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "유가증권",
            ldnmAdrs: "보관중인 건설공제조합 출자증권");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.RecentTrades.Should().NotBeNull();
        result.RecentTrades.Should().BeEmpty();
        await _tradeRepository.DidNotReceive().FindByLocationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatchingTrades_ShouldReturnEmptyTradesAsync()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "토지",
            ldnmAdrs: "경기도 남양주시 진접읍 내각리 165-77");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);
        _tradeRepository.FindByLocationAsync("내각리", "165-77", 20, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RealEstateTrade>());

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.RecentTrades.Should().NotBeNull();
        result.RecentTrades.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallFindByLocationWithLimit20Async()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "부곡 롯데캐슬",
            ldnmAdrs: "부산광역시 금정구 부곡동 970 롯데캐슬디아망");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);
        _tradeRepository.FindByLocationAsync("부곡동", "970", 20, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RealEstateTrade>());

        await _sut.ExecuteAsync(1L);

        await _tradeRepository.Received(1).FindByLocationAsync(
            "부곡동",
            "970",
            20,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapTradeResponseFieldsCorrectlyAsync()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "부곡 롯데캐슬",
            ldnmAdrs: "부산광역시 금정구 부곡동 970 롯데캐슬디아망");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        List<RealEstateTrade> trades = [CreateTrade("26260", "부곡동", "970", 85000, 2026, 3, 15)];
        _tradeRepository.FindByLocationAsync("부곡동", "970", 20, Arg.Any<CancellationToken>())
            .Returns(trades);

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        RealEstateTradeResponse trade = result.RecentTrades![0];
        trade.UmdNm.Should().Be("부곡동");
        trade.Jibun.Should().Be("970");
        trade.DealAmount.Should().Be(85000);
        trade.DealYear.Should().Be(2026);
        trade.DealMonth.Should().Be(3);
        trade.DealDay.Should().Be(15);
        trade.BuildingName.Should().Be("래미안");
        trade.Area.Should().Be(84.99m);
    }

    [Fact]
    public async Task ExecuteAsync_DetailResponse_ShouldNotPopulateLatestTradeFieldsAsync()
    {
        KamcoAuctionItem entity = CreateEntity(
            1,
            100,
            200,
            300,
            "부곡 롯데캐슬",
            ldnmAdrs: "부산광역시 금정구 부곡동 970 롯데캐슬디아망");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        List<RealEstateTrade> trades = [CreateTrade("26260", "부곡동", "970", 85000, 2026, 1, 15)];
        _tradeRepository.FindByLocationAsync("부곡동", "970", 20, Arg.Any<CancellationToken>())
            .Returns(trades);

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.RecentTrades.Should().HaveCount(1);
        result.LatestTradeAmount.Should().BeNull();
        result.LatestTradeDate.Should().BeNull();
    }

    private static KamcoAuctionItem CreateEntity(
        long id,
        long plnmNo,
        long pbctNo,
        long cltrNo,
        string cltrNm,
        long minBidPrc = 100,
        long apslAsesAvgAmt = 200,
        string ldnmAdrs = "Addr1")
    {
        var item = KamcoAuctionItem.Create(
            plnmNo,
            pbctNo,
            cltrNo,
            cltrNm,
            "Category",
            ldnmAdrs,
            "Addr2",
            minBidPrc,
            apslAsesAvgAmt,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);

        typeof(KamcoAuctionItem)
            .GetProperty(nameof(KamcoAuctionItem.Id))!
            .SetValue(item, id);

        return item;
    }

    private static RealEstateTrade CreateTrade(
        string lawdCd,
        string umdNm,
        string jibun,
        long dealAmount,
        int dealYear,
        int dealMonth,
        int dealDay)
    {
        return RealEstateTrade.Create(
            lawdCd,
            1,
            "래미안",
            jibun,
            umdNm,
            dealAmount,
            dealYear,
            dealMonth,
            dealDay,
            84.99m,
            10,
            2020);
    }
}
