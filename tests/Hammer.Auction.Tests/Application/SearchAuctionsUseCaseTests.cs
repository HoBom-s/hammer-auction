using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.SearchAuctions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="SearchAuctionsUseCase"/>.
/// </summary>
public sealed class SearchAuctionsUseCaseTests
{
    private readonly IKamcoAuctionItemRepository _kamcoRepo = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly IInstitutionAuctionItemRepository _institutionRepo = Substitute.For<IInstitutionAuctionItemRepository>();
    private readonly ISearchLogRepository _searchLogRepo = Substitute.For<ISearchLogRepository>();
    private readonly SearchAuctionsUseCase _sut;

    public SearchAuctionsUseCaseTests()
    {
        _sut = new SearchAuctionsUseCase(_kamcoRepo, _institutionRepo, _searchLogRepo);
        SetupEmptyDefaults();
    }

    [Fact]
    public async Task ExecuteAsync_WithKeyword_ShouldReturnMergedResultsAsync()
    {
        List<KamcoAuctionItem> kamcoItems = [CreateKamcoEntity(1, "Kamco Item")];
        List<InstitutionAuctionItem> institutionItems = [CreateInstitutionEntity(1, "Institution Item")];

        _kamcoRepo.SearchAsync("서울", 20, Arg.Any<CancellationToken>()).Returns((kamcoItems, 1));
        _institutionRepo.SearchAsync("서울", 20, Arg.Any<CancellationToken>()).Returns((institutionItems, 1));

        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("서울"), "user-1");

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyKeyword_ShouldReturnEmptyAsync()
    {
        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest(""), "user-1");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullKeyword_ShouldReturnEmptyAsync()
    {
        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest(null), "user-1");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceKeyword_ShouldReturnEmptyAsync()
    {
        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("   "), "user-1");

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogSearchKeywordAsync()
    {
        _kamcoRepo.SearchAsync("아파트", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));
        _institutionRepo.SearchAsync("아파트", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 0));

        await _sut.ExecuteAsync(new SearchAuctionsRequest("아파트"), "user-1");

        _searchLogRepo.Received(1).Add(Arg.Is<SearchLog>(l => l.UserId == "user-1"));
        await _searchLogRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyKeyword_ShouldNotLogAsync()
    {
        await _sut.ExecuteAsync(new SearchAuctionsRequest(""), "user-1");

        _searchLogRepo.DidNotReceive().Add(Arg.Any<SearchLog>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateTotalPagesAsync()
    {
        _kamcoRepo.SearchAsync("토지", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 30));
        _institutionRepo.SearchAsync("토지", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 15));

        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("토지"), "user-1");

        result.TotalCount.Should().Be(45);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ShouldPassCorrectLimitAsync()
    {
        _kamcoRepo.SearchAsync("강남", 40, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));
        _institutionRepo.SearchAsync("강남", 40, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 0));

        await _sut.ExecuteAsync(new SearchAuctionsRequest("강남", Page: 2, Size: 20), "user-1");

        await _kamcoRepo.Received(1).SearchAsync("강남", 40, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSortByPbctBegnDtmDescAsync()
    {
        // KamcoEntity created with "20260401090000" (earlier) and institution with "20260415090000" (later)
        List<KamcoAuctionItem> kamcoItems = [CreateKamcoEntity(1, "Earlier")];
        List<InstitutionAuctionItem> institutionItems = [CreateInstitutionEntity(1, "Later")];

        _kamcoRepo.SearchAsync("test", 20, Arg.Any<CancellationToken>()).Returns((kamcoItems, 1));
        _institutionRepo.SearchAsync("test", 20, Arg.Any<CancellationToken>()).Returns((institutionItems, 1));

        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("test"), "user-1");

        // Institution item (April 15) should come before Kamco item (April 1)
        result.Items[0].Name.Should().Be("Later");
        result.Items[1].Name.Should().Be("Earlier");
    }

    [Fact]
    public async Task ExecuteAsync_KamcoItem_ShouldSetSourceToKamcoAsync()
    {
        List<KamcoAuctionItem> kamcoItems = [CreateKamcoEntity(1, "Test")];
        _kamcoRepo.SearchAsync("test", 20, Arg.Any<CancellationToken>()).Returns((kamcoItems, 1));

        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("test"), "user-1");

        result.Items[0].Source.Should().Be("Kamco");
        result.Items[0].MinBidPrice.Should().Be(100);
        result.Items[0].Address.Should().Be("Addr1");
    }

    [Fact]
    public async Task ExecuteAsync_InstitutionItem_ShouldHaveNullOptionalFieldsAsync()
    {
        List<InstitutionAuctionItem> institutionItems = [CreateInstitutionEntity(1, "Test")];
        _institutionRepo.SearchAsync("test", 20, Arg.Any<CancellationToken>()).Returns((institutionItems, 1));

        PagedResponse<UnifiedAuctionItemResponse> result =
            await _sut.ExecuteAsync(new SearchAuctionsRequest("test"), "user-1");

        result.Items[0].Source.Should().Be("Institution");
        result.Items[0].MinBidPrice.Should().BeNull();
        result.Items[0].Address.Should().BeNull();
        result.Items[0].Status.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!, "user-1");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPage_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(new SearchAuctionsRequest("test", Page: 0), "user-1");

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimKeywordAsync()
    {
        _kamcoRepo.SearchAsync("서울", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<KamcoAuctionItem>(), 0));
        _institutionRepo.SearchAsync("서울", 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 0));

        await _sut.ExecuteAsync(new SearchAuctionsRequest("  서울  "), "user-1");

        await _kamcoRepo.Received(1).SearchAsync("서울", 20, Arg.Any<CancellationToken>());
    }

    private static KamcoAuctionItem CreateKamcoEntity(long plnmNo, string cltrNm)
    {
        return KamcoAuctionItem.Create(
            plnmNo,
            1,
            1,
            cltrNm,
            "Category",
            "Addr1",
            "Addr2",
            100,
            200,
            "Method",
            "Status",
            "20260401090000",
            "20260402090000",
            0,
            0,
            null);
    }

    private static InstitutionAuctionItem CreateInstitutionEntity(long plnmNo, string plnmNm)
    {
        return InstitutionAuctionItem.Create(
            plnmNo,
            1,
            "01",
            "Kind",
            "01",
            "BidDvsn",
            plnmNm,
            "Org",
            "20260101",
            "ORG-001",
            "MNMT-001",
            "01",
            "Method",
            "01",
            "TotAmt",
            "01",
            "Dpsl",
            "01",
            "Prpt",
            "20260415090000",
            "20260416090000",
            "20260417090000",
            "CTG01",
            "Category");
    }

    private void SetupEmptyDefaults()
    {
        _kamcoRepo.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()).Returns((Array.Empty<KamcoAuctionItem>(), 0));
        _institutionRepo.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()).Returns((Array.Empty<InstitutionAuctionItem>(), 0));
    }
}
