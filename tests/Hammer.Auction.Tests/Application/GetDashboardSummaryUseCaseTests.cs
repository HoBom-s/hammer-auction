using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetDashboardSummary;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetDashboardSummaryUseCase"/>.
/// </summary>
public sealed class GetDashboardSummaryUseCaseTests
{
    private readonly IKamcoAuctionItemRepository _repository = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly GetDashboardSummaryUseCase _sut;

    public GetDashboardSummaryUseCaseTests()
    {
        _sut = new GetDashboardSummaryUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllThreeCategoriesAsync()
    {
        SetupEmptyRepository();

        DashboardSummaryResponse result = await _sut.ExecuteAsync();

        result.Categories.Should().HaveCount(3);
        result.Categories.Select(c => c.Category)
            .Should().Contain(["부동산", "동산", "기타"]);
    }

    [Fact]
    public async Task ExecuteAsync_WithData_ShouldAggregateTotalsAsync()
    {
        List<(string, int)> totals =
        [
            ("토지 / 대지", 10),
            ("주거용건물 / 아파트", 5),
            ("자동차 / 승용차", 3),
            ("유가증권", 2),
        ];

        _repository.CountByCtgrFullNmAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(totals);
        _repository.CountByCtgrFullNmAsync(
                Arg.Is<DateTimeOffset?>(d => d.HasValue),
                Arg.Is<DateTimeOffset?>(d => d.HasValue),
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<(string, int)>());

        DashboardSummaryResponse result = await _sut.ExecuteAsync();

        DashboardCategorySummary realEstate = result.Categories.First(c => c.Category == "부동산");
        realEstate.TotalCount.Should().Be(15);

        DashboardCategorySummary movable = result.Categories.First(c => c.Category == "동산");
        movable.TotalCount.Should().Be(3);

        DashboardCategorySummary other = result.Categories.First(c => c.Category == "기타");
        other.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateDailyChangeAsync()
    {
        SetupTotalCounts();

        _repository.CountByCtgrFullNmAsync(
                Arg.Is<DateTimeOffset?>(d => d.HasValue),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    DateTimeOffset start = callInfo.ArgAt<DateTimeOffset?>(0)!.Value;
                    DateTimeOffset end = callInfo.ArgAt<DateTimeOffset?>(1)!.Value;
                    TimeSpan span = end - start;

                    // Today's window (1 day span, later start)
                    if (span.TotalDays is > 0.9 and < 1.1)
                    {
                        DateTimeOffset nowKst = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(9));
                        var todayStart = new DateTimeOffset(nowKst.Date, TimeSpan.FromHours(9));

                        if (start >= todayStart.AddHours(-1))
                            return new List<(string, int)> { ("토지 / 대지", 5) };

                        // Yesterday's window
                        return new List<(string, int)> { ("토지 / 대지", 3) };
                    }

                    return Array.Empty<(string, int)>();
                });

        DashboardSummaryResponse result = await _sut.ExecuteAsync();

        DashboardCategorySummary realEstate = result.Categories.First(c => c.Category == "부동산");
        realEstate.DailyChange.Should().Be(2); // today(5) - yesterday(3)
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRepository_ShouldReturnZeroCountsAsync()
    {
        SetupEmptyRepository();

        DashboardSummaryResponse result = await _sut.ExecuteAsync();

        result.Categories.Should().AllSatisfy(c =>
        {
            c.TotalCount.Should().Be(0);
            c.DailyChange.Should().Be(0);
        });
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryThreeTimesAsync()
    {
        SetupEmptyRepository();

        await _sut.ExecuteAsync();

        await _repository.Received(3).CountByCtgrFullNmAsync(
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<CancellationToken>());
    }

    private void SetupEmptyRepository()
    {
        _repository.CountByCtgrFullNmAsync(
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<(string, int)>());
    }

    private void SetupTotalCounts()
    {
        _repository.CountByCtgrFullNmAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<(string, int)> { ("토지 / 대지", 100) });
    }
}
