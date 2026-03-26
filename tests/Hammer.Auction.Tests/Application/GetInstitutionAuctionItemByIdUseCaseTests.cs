using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetInstitutionAuctionItemByIdUseCase"/>.
/// </summary>
public sealed class GetInstitutionAuctionItemByIdUseCaseTests
{
    private readonly IInstitutionAuctionItemRepository _repository = Substitute.For<IInstitutionAuctionItemRepository>();
    private readonly GetInstitutionAuctionItemByIdUseCase _sut;

    public GetInstitutionAuctionItemByIdUseCaseTests()
    {
        _sut = new GetInstitutionAuctionItemByIdUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingItem_ShouldReturnResponseAsync()
    {
        InstitutionAuctionItem entity = CreateEntity(1, 100, 200);
        _repository.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(entity);

        InstitutionAuctionItemResponse result = await _sut.ExecuteAsync(1L);

        result.Should().NotBeNull();
        result.PlnmNo.Should().Be(100);
        result.PbctNo.Should().Be(200);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentItem_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetByIdAsync(999L, Arg.Any<CancellationToken>()).Returns((InstitutionAuctionItem?)null);

        Func<Task> act = () => _sut.ExecuteAsync(999L);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999*");
    }

    private static InstitutionAuctionItem CreateEntity(long id, long plnmNo, long pbctNo)
    {
        var item = InstitutionAuctionItem.Create(
            plnmNo,
            pbctNo,
            "01",
            "Kind",
            "01",
            "BidDvsn",
            "PlnmNm",
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

        typeof(InstitutionAuctionItem)
            .GetProperty(nameof(InstitutionAuctionItem.Id))!
            .SetValue(item, id);

        return item;
    }
}
