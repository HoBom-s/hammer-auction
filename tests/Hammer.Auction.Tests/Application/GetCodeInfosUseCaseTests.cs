using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetCodeInfosUseCase"/>.
/// </summary>
public sealed class GetCodeInfosUseCaseTests
{
    private readonly IOnbidCodeInfoRepository _repository = Substitute.For<IOnbidCodeInfoRepository>();
    private readonly GetCodeInfosUseCase _sut;

    public GetCodeInfosUseCaseTests()
    {
        _sut = new GetCodeInfosUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<OnbidCodeInfo> items = [CreateEntity("CTG01", "토지"), CreateEntity("CTG02", "건물")];
        _repository.GetPagedAsync(1, 100, null, Arg.Any<CancellationToken>())
            .Returns((items, 2));

        PagedResponse<OnbidCodeInfoResponse> result = await _sut.ExecuteAsync(new GetCodeInfosRequest());

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.Size.Should().Be(100);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePages_ShouldCalculateTotalPagesAsync()
    {
        List<OnbidCodeInfo> items = [CreateEntity("CTG01", "토지")];
        _repository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns((items, 25));

        PagedResponse<OnbidCodeInfoResponse> result = await _sut.ExecuteAsync(
            new GetCodeInfosRequest(Page: 1, Size: 10));

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyPageAsync()
    {
        _repository.GetPagedAsync(1, 100, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<OnbidCodeInfo>(), 0));

        PagedResponse<OnbidCodeInfoResponse> result = await _sut.ExecuteAsync(new GetCodeInfosRequest());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassParentIdFilterToRepositoryAsync()
    {
        _repository.GetPagedAsync(1, 100, "ROOT", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<OnbidCodeInfo>(), 0));

        GetCodeInfosRequest request = new(ParentId: "ROOT");

        await _sut.ExecuteAsync(request);

        await _repository.Received(1).GetPagedAsync(1, 100, "ROOT", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static OnbidCodeInfo CreateEntity(string ctgrId, string ctgrNm)
    {
        return OnbidCodeInfo.Create(ctgrId, ctgrNm, "ROOT", "전체");
    }
}
