using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetPopularSearchTerms;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetPopularSearchTermsUseCase"/>.
/// </summary>
public sealed class GetPopularSearchTermsUseCaseTests
{
    private readonly ISearchLogRepository _repository = Substitute.For<ISearchLogRepository>();
    private readonly GetPopularSearchTermsUseCase _sut;

    public GetPopularSearchTermsUseCaseTests()
    {
        _sut = new GetPopularSearchTermsUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPopularTermsAsync()
    {
        _repository.GetPopularAsync(7, 10, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)> { ("아파트", 50), ("토지", 30) });

        IReadOnlyList<PopularSearchTermResponse> result =
            await _sut.ExecuteAsync(new GetPopularSearchTermsRequest());

        result.Should().HaveCount(2);
        result[0].Keyword.Should().Be("아파트");
        result[0].Count.Should().Be(50);
        result[1].Keyword.Should().Be("토지");
        result[1].Count.Should().Be(30);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyListAsync()
    {
        _repository.GetPopularAsync(7, 10, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)>());

        IReadOnlyList<PopularSearchTermResponse> result =
            await _sut.ExecuteAsync(new GetPopularSearchTermsRequest());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassParametersToRepositoryAsync()
    {
        _repository.GetPopularAsync(30, 5, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)>());

        await _sut.ExecuteAsync(new GetPopularSearchTermsRequest(Days: 30, Limit: 5));

        await _repository.Received(1).GetPopularAsync(30, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseDefaultParametersAsync()
    {
        _repository.GetPopularAsync(7, 10, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)>());

        await _sut.ExecuteAsync(new GetPopularSearchTermsRequest());

        await _repository.Received(1).GetPopularAsync(7, 10, Arg.Any<CancellationToken>());
    }
}
