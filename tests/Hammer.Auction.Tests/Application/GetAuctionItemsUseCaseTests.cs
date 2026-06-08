using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetAuctionItemsUseCase"/>.
/// </summary>
public sealed class GetAuctionItemsUseCaseTests
{
    private readonly IKamcoAuctionItemRepository _repository = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly IRealEstateTradeRepository _tradeRepository = Substitute.For<IRealEstateTradeRepository>();
    private readonly GetAuctionItemsUseCase _sut;

    public GetAuctionItemsUseCaseTests()
    {
        _sut = new GetAuctionItemsUseCase(_repository, _tradeRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<KamcoAuctionItem> items = [CreateEntity(1, "Item A"), CreateEntity(2, "Item B")];
        SetupRepository(items, 2);
        SetupTradesFallback();

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.Size.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePages_ShouldCalculateTotalPagesAsync()
    {
        List<KamcoAuctionItem> items = [CreateEntity(1, "Item")];
        _repository.GetPagedAsync(1, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 25));
        SetupTradesFallback();

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(
            new GetAuctionItemsRequest(Page: 1, Size: 10));

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyPageAsync()
    {
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassFilterParametersToRepositoryAsync()
    {
        _repository.GetPagedAsync(2, 5, "진행", "토지", "서울", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));

        GetAuctionItemsRequest request = new(
            Page: 2,
            Size: 5,
            Status: "진행",
            Category: "토지",
            Keyword: "서울");

        await _sut.ExecuteAsync(request);

        await _repository.Received(1).GetPagedAsync(2, 5, "진행", "토지", "서울", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldEnrichItemsWithLatestTradeAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "부곡 롯데캐슬", "부산광역시 금정구 부곡동 970 롯데캐슬디아망"),
        ];
        SetupRepository(items, 1);

        RealEstateTrade trade = CreateTrade("부곡동", "970", 85000, 2026, 1);
        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { trade });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[0].LatestTradeDate.Should().Be("2026-01");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnparsableAddress_ShouldLeaveTradeFieldsNullAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "유가증권", "보관중인 건설공제조합 출자증권"),
        ];
        SetupRepository(items, 1);

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().BeNull();
        result.Items[0].LatestTradeDate.Should().BeNull();
        await _tradeRepository.DidNotReceive().FindByLocationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatchingTrade_ShouldLeaveTradeFieldsNullAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "토지", "경기도 남양주시 진접읍 내각리 165-77"),
        ];
        SetupRepository(items, 1);

        _tradeRepository.FindByLocationAsync("내각리", "165-77", 1, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<RealEstateTrade>());

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().BeNull();
        result.Items[0].LatestTradeDate.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleItems_ShouldEnrichEachWithCorrectTradeAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "부곡 롯데캐슬", "부산광역시 금정구 부곡동 970 롯데캐슬디아망"),
            CreateEntity(2, "내각리 토지", "경기도 남양주시 진접읍 내각리 165-77"),
        ];
        SetupRepository(items, 2);

        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("부곡동", "970", 85000, 2026, 1) });
        _tradeRepository.FindByLocationAsync("내각리", "165-77", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("내각리", "165-77", 12000, 2025, 12) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[0].LatestTradeDate.Should().Be("2026-01");
        result.Items[1].LatestTradeAmount.Should().Be(12000);
        result.Items[1].LatestTradeDate.Should().Be("2025-12");
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateAddress_ShouldQueryOnceAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "101동", "부산광역시 금정구 부곡동 970 롯데캐슬 101동"),
            CreateEntity(2, "102동", "부산광역시 금정구 부곡동 970 롯데캐슬 101동"),
        ];
        SetupRepository(items, 2);

        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("부곡동", "970", 85000, 2026, 3) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[1].LatestTradeAmount.Should().Be(85000);
        await _tradeRepository.Received(1).FindByLocationAsync(
            "부곡동",
            "970",
            1,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedAddresses_ShouldEnrichOnlyParsableAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "부곡 롯데캐슬", "부산광역시 금정구 부곡동 970 롯데캐슬디아망"),
            CreateEntity(2, "유가증권", "보관중인 건설공제조합 출자증권"),
        ];
        SetupRepository(items, 2);

        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("부곡동", "970", 85000, 2026, 1) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[1].LatestTradeAmount.Should().BeNull();
        result.Items[1].LatestTradeDate.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ListResponse_ShouldNotPopulateRecentTradesAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "부곡 롯데캐슬", "부산광역시 금정구 부곡동 970 롯데캐슬디아망"),
        ];
        SetupRepository(items, 1);

        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("부곡동", "970", 85000, 2026, 1) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[0].RecentTrades.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_DifferentAddressesSameLocation_ShouldDeduplicateQueryAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "101동", "부산광역시 금정구 부곡동 970 롯데캐슬 101동"),
            CreateEntity(2, "102동", "부산광역시 금정구 부곡동 970 롯데캐슬 102동"),
        ];
        SetupRepository(items, 2);

        _tradeRepository.FindByLocationAsync("부곡동", "970", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("부곡동", "970", 85000, 2026, 3) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeAmount.Should().Be(85000);
        result.Items[1].LatestTradeAmount.Should().Be(85000);
        await _tradeRepository.Received(1).FindByLocationAsync(
            "부곡동",
            "970",
            1,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AllUnparsable_ShouldNotCallTradeRepositoryAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "유가증권A", "보관중인 건설공제조합 출자증권"),
            CreateEntity(2, "유가증권B", "서울특별시 강남구 테헤란로 152"),
        ];
        SetupRepository(items, 2);

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items.Should().AllSatisfy(item =>
        {
            item.LatestTradeAmount.Should().BeNull();
            item.LatestTradeDate.Should().BeNull();
        });
        await _tradeRepository.DidNotReceive().FindByLocationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFormatSingleDigitMonthWithLeadingZeroAsync()
    {
        List<KamcoAuctionItem> items =
        [
            CreateEntity(1, "토지", "경기도 남양주시 진접읍 내각리 165-77"),
        ];
        SetupRepository(items, 1);

        _tradeRepository.FindByLocationAsync("내각리", "165-77", 1, Arg.Any<CancellationToken>())
            .Returns(new[] { CreateTrade("내각리", "165-77", 50000, 2025, 9) });

        PagedResponse<KamcoAuctionItemResponse> result = await _sut.ExecuteAsync(new GetAuctionItemsRequest());

        result.Items[0].LatestTradeDate.Should().Be("2025-09");
    }

    private static KamcoAuctionItem CreateEntity(
        long plnmNo,
        string cltrNm,
        string ldnmAdrs = "Addr1")
    {
        return KamcoAuctionItem.Create(
            plnmNo,
            1,
            1,
            cltrNm,
            "Category",
            ldnmAdrs,
            "Addr2",
            100,
            200,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);
    }

    private static RealEstateTrade CreateTrade(
        string umdNm,
        string jibun,
        long dealAmount,
        int dealYear,
        int dealMonth)
    {
        return RealEstateTrade.Create(
            "26260",
            1,
            "래미안",
            jibun,
            umdNm,
            dealAmount,
            dealYear,
            dealMonth,
            15,
            84.99m,
            10,
            2020);
    }

    private void SetupRepository(List<KamcoAuctionItem> items, int totalCount)
    {
        _repository.GetPagedAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns((items, totalCount));
    }

    private void SetupTradesFallback()
    {
        _tradeRepository.FindByLocationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()).Returns(Array.Empty<RealEstateTrade>());
    }
}
