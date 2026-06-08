using FluentAssertions;
using Hammer.Auction.Application.UseCases.DeleteSearchHistory;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="DeleteSearchHistoryUseCase"/>.
/// </summary>
public sealed class DeleteSearchHistoryUseCaseTests
{
    private readonly ISearchLogRepository _repository = Substitute.For<ISearchLogRepository>();
    private readonly DeleteSearchHistoryUseCase _sut;

    public DeleteSearchHistoryUseCaseTests()
    {
        _sut = new DeleteSearchHistoryUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteByUserAndReturnCountAsync()
    {
        _repository.DeleteByUserAsync("user-1", Arg.Any<CancellationToken>()).Returns(5);

        var deleted = await _sut.ExecuteAsync("user-1");

        deleted.Should().Be(5);
        await _repository.Received(1).DeleteByUserAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoHistory_ShouldReturnZeroAsync()
    {
        _repository.DeleteByUserAsync("user-new", Arg.Any<CancellationToken>()).Returns(0);

        var deleted = await _sut.ExecuteAsync("user-new");

        deleted.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithInvalidUserId_ShouldThrowAsync(string? userId)
    {
        Func<Task> act = () => _sut.ExecuteAsync(userId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
