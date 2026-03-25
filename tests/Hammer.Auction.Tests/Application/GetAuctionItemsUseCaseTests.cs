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
    private readonly GetAuctionItemsUseCase _sut;

    public GetAuctionItemsUseCaseTests()
    {
        _sut = new GetAuctionItemsUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<KamcoAuctionItem> items = [CreateEntity(1, "Item A"), CreateEntity(2, "Item B")];
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

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

    private static KamcoAuctionItem CreateEntity(long plnmNo, string cltrNm)
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
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);
    }
}
