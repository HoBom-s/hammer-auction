using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetRealEstateTrades;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetRealEstateTradesUseCase"/>.
/// </summary>
public sealed class GetRealEstateTradesUseCaseTests
{
    private readonly IRealEstateTradeRepository _repository = Substitute.For<IRealEstateTradeRepository>();
    private readonly GetRealEstateTradesUseCase _sut;

    public GetRealEstateTradesUseCaseTests()
    {
        _sut = new GetRealEstateTradesUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<RealEstateTrade> items =
        [
            CreateEntity("11110", 1, "래미안", "123-4"),
            CreateEntity("11110", 1, "자이", "456-7"),
        ];
        _repository.GetPagedAsync(1, 20, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

        PagedResponse<RealEstateTradeResponse> result = await _sut.ExecuteAsync(new GetRealEstateTradesRequest());

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.Size.Should().Be(20);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePages_ShouldCalculateTotalPagesAsync()
    {
        List<RealEstateTrade> items = [CreateEntity("11110", 1, "래미안", "123-4")];
        _repository.GetPagedAsync(1, 10, null, null, Arg.Any<CancellationToken>())
            .Returns((items, 25));

        PagedResponse<RealEstateTradeResponse> result = await _sut.ExecuteAsync(
            new GetRealEstateTradesRequest(Page: 1, Size: 10));

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyPageAsync()
    {
        _repository.GetPagedAsync(1, 20, null, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<RealEstateTrade>(), 0));

        PagedResponse<RealEstateTradeResponse> result = await _sut.ExecuteAsync(new GetRealEstateTradesRequest());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassFiltersToRepositoryAsync()
    {
        _repository.GetPagedAsync(1, 20, "11110", 1, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<RealEstateTrade>(), 0));

        GetRealEstateTradesRequest request = new(LawdCd: "11110", PropertyType: 1);

        await _sut.ExecuteAsync(request);

        await _repository.Received(1).GetPagedAsync(1, 20, "11110", 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static RealEstateTrade CreateEntity(
        string lawdCd,
        int propertyType,
        string buildingName,
        string jibun)
    {
        return RealEstateTrade.Create(
            lawdCd,
            propertyType,
            buildingName,
            jibun,
            "종로동",
            85000,
            2025,
            3,
            15,
            84.99m,
            10,
            2020);
    }
}
