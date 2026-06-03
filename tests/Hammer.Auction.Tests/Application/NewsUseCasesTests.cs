using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Application.UseCases.GetNewsById;
using Hammer.Auction.Application.UseCases.GetNewsList;
using Hammer.Auction.Application.UseCases.GetRecentNews;
using Hammer.Auction.Application.UseCases.SearchNews;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for the news use cases that consume hammer-support.
/// </summary>
public sealed class NewsUseCasesTests
{
    private readonly INewsClient _newsClient = Substitute.For<INewsClient>();

    [Fact]
    public async Task GetRecentNews_WithValidCount_ShouldReturnListAsync()
    {
        _newsClient.GetRecentAsync(5, Arg.Any<CancellationToken>()).Returns([CreateNews()]);
        GetRecentNewsUseCase sut = new(_newsClient);

        IReadOnlyList<NewsResponse> result = await sut.ExecuteAsync(5);

        result.Should().ContainSingle();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public async Task GetRecentNews_WithInvalidCount_ShouldThrowBadRequestAsync(int count)
    {
        GetRecentNewsUseCase sut = new(_newsClient);

        Func<Task> act = () => sut.ExecuteAsync(count);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GetNewsById_WhenFound_ShouldReturnNewsAsync()
    {
        var id = Guid.NewGuid();
        _newsClient.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(CreateNews());
        GetNewsByIdUseCase sut = new(_newsClient);

        NewsResponse result = await sut.ExecuteAsync(id);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNewsById_WhenMissing_ShouldThrowNotFoundAsync()
    {
        _newsClient.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NewsResponse?)null);
        GetNewsByIdUseCase sut = new(_newsClient);

        Func<Task> act = () => sut.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetNewsList_WithValidArgs_ShouldReturnPageAsync()
    {
        _newsClient.GetPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<NewsResponse>([CreateNews()], 1, 20, 1, 1));
        GetNewsListUseCase sut = new(_newsClient);

        PagedResponse<NewsResponse> result = await sut.ExecuteAsync(1, 20);

        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetNewsList_WithInvalidArgs_ShouldThrowBadRequestAsync(int page, int size)
    {
        GetNewsListUseCase sut = new(_newsClient);

        Func<Task> act = () => sut.ExecuteAsync(page, size);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task SearchNews_WithValidKeyword_ShouldReturnPageAsync()
    {
        _newsClient.SearchByTitleAsync("경매", 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<NewsResponse>([CreateNews()], 1, 20, 1, 1));
        SearchNewsUseCase sut = new(_newsClient);

        PagedResponse<NewsResponse> result = await sut.ExecuteAsync("경매", 1, 20);

        result.Items.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchNews_WithEmptyKeyword_ShouldThrowBadRequestAsync(string keyword)
    {
        SearchNewsUseCase sut = new(_newsClient);

        Func<Task> act = () => sut.ExecuteAsync(keyword, 1, 20);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    private static NewsResponse CreateNews() =>
        new(Guid.NewGuid(), "경매", "제목", "https://example.com/1", "https://example.com/1", "요약", DateTimeOffset.UtcNow);
}
