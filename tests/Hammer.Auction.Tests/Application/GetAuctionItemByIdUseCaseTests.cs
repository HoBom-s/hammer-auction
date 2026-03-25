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
    private readonly GetAuctionItemByIdUseCase _sut;

    public GetAuctionItemByIdUseCaseTests()
    {
        _sut = new GetAuctionItemByIdUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingItem_ShouldReturnResponseAsync()
    {
        KamcoAuctionItem entity = CreateEntity(1, 100, 200, 300, "Test Item");
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

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

        KamcoAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.DiscountRate.Should().Be(20.0);
    }

    private static KamcoAuctionItem CreateEntity(
        long id,
        long plnmNo,
        long pbctNo,
        long cltrNo,
        string cltrNm,
        long minBidPrc = 100,
        long apslAsesAvgAmt = 200)
    {
        var item = KamcoAuctionItem.Create(
            plnmNo,
            pbctNo,
            cltrNo,
            cltrNm,
            "Category",
            "Addr1",
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
}
