using FluentAssertions;
using Hammer.Auction.Application.UseCases.GetRecentSearchTerms;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetRecentSearchTermsUseCase"/>.
/// </summary>
public sealed class GetRecentSearchTermsUseCaseTests
{
    private readonly ISearchLogRepository _repository = Substitute.For<ISearchLogRepository>();
    private readonly GetRecentSearchTermsUseCase _sut;

    public GetRecentSearchTermsUseCaseTests()
    {
        _sut = new GetRecentSearchTermsUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnRecentKeywordsAsync()
    {
        _repository.GetRecentByUserAsync("user-1", 10, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "아파트", "토지", "강남" });

        IReadOnlyList<string> result =
            await _sut.ExecuteAsync(new GetRecentSearchTermsRequest("user-1"));

        result.Should().HaveCount(3);
        result[0].Should().Be("아파트");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyListAsync()
    {
        _repository.GetRecentByUserAsync("user-1", 10, Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        IReadOnlyList<string> result =
            await _sut.ExecuteAsync(new GetRecentSearchTermsRequest("user-1"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassParametersToRepositoryAsync()
    {
        _repository.GetRecentByUserAsync("user-42", 5, Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        await _sut.ExecuteAsync(new GetRecentSearchTermsRequest("user-42", Limit: 5));

        await _repository.Received(1).GetRecentByUserAsync("user-42", 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseDefaultLimitAsync()
    {
        _repository.GetRecentByUserAsync("user-1", 10, Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        await _sut.ExecuteAsync(new GetRecentSearchTermsRequest("user-1"));

        await _repository.Received(1).GetRecentByUserAsync("user-1", 10, Arg.Any<CancellationToken>());
    }
}
