using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetInstitutionAuctionItemsUseCase"/>.
/// </summary>
public sealed class GetInstitutionAuctionItemsUseCaseTests
{
    private readonly IInstitutionAuctionItemRepository _repository = Substitute.For<IInstitutionAuctionItemRepository>();
    private readonly GetInstitutionAuctionItemsUseCase _sut;

    public GetInstitutionAuctionItemsUseCaseTests()
    {
        _sut = new GetInstitutionAuctionItemsUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<InstitutionAuctionItem> items = [CreateEntity(1, "Item A"), CreateEntity(2, "Item B")];
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

        PagedResponse<InstitutionAuctionItemResponse> result = await _sut.ExecuteAsync(new GetInstitutionAuctionItemsRequest());

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.Size.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePages_ShouldCalculateTotalPagesAsync()
    {
        List<InstitutionAuctionItem> items = [CreateEntity(1, "Item")];
        _repository.GetPagedAsync(1, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 25));

        PagedResponse<InstitutionAuctionItemResponse> result = await _sut.ExecuteAsync(
            new GetInstitutionAuctionItemsRequest(Page: 1, Size: 10));

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyPageAsync()
    {
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 0));

        PagedResponse<InstitutionAuctionItemResponse> result = await _sut.ExecuteAsync(new GetInstitutionAuctionItemsRequest());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassFilterParametersToRepositoryAsync()
    {
        _repository.GetPagedAsync(2, 5, "한국자산관리공사", "토지", "서울", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<InstitutionAuctionItem>(), 0));

        GetInstitutionAuctionItemsRequest request = new(
            Page: 2,
            Size: 5,
            Org: "한국자산관리공사",
            Category: "토지",
            Keyword: "서울");

        await _sut.ExecuteAsync(request);

        await _repository.Received(1).GetPagedAsync(2, 5, "한국자산관리공사", "토지", "서울", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static InstitutionAuctionItem CreateEntity(long plnmNo, string plnmNm)
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
            "20260101090000",
            "20260102090000",
            "20260103090000",
            "CTG01",
            "Category");
    }
}
