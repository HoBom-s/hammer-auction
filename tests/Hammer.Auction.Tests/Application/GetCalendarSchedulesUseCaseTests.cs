using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetCalendarSchedules;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetCalendarSchedulesUseCase"/>.
/// </summary>
public sealed class GetCalendarSchedulesUseCaseTests
{
    private readonly IKamcoAuctionItemRepository _kamcoRepo = Substitute.For<IKamcoAuctionItemRepository>();
    private readonly IInstitutionAuctionItemRepository _institutionRepo = Substitute.For<IInstitutionAuctionItemRepository>();
    private readonly GetCalendarSchedulesUseCase _sut;

    public GetCalendarSchedulesUseCaseTests()
    {
        _sut = new GetCalendarSchedulesUseCase(_kamcoRepo, _institutionRepo);
        SetupEmptyDefaults();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnYearAndMonthAsync()
    {
        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Year.Should().Be(2026);
        result.Month.Should().Be(4);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoItems_ShouldReturnEmptySchedulesAsync()
    {
        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Schedules.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGroupItemsByKstDateAsync()
    {
        // KST 2026-04-10 09:00 = UTC 2026-04-10 00:00
        List<KamcoAuctionItem> kamcoItems =
        [
            CreateKamcoEntity(1, "Item A", "20260410090000", "20260411090000"),
            CreateKamcoEntity(2, "Item B", "20260410180000", "20260411180000"),
        ];
        _kamcoRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(kamcoItems);

        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Schedules.Should().ContainKey("2026-04-10");
        result.Schedules["2026-04-10"].Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMergeKamcoAndInstitutionItemsAsync()
    {
        List<KamcoAuctionItem> kamcoItems = [CreateKamcoEntity(1, "Kamco", "20260415090000", "20260416090000")];
        List<InstitutionAuctionItem> institutionItems = [CreateInstitutionEntity(1, "Institution", "20260415090000", "20260416090000")];

        _kamcoRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(kamcoItems);
        _institutionRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(institutionItems);

        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Schedules["2026-04-15"].Should().HaveCount(2);
        result.Schedules["2026-04-15"].Select(x => x.Source).Should().Contain("Kamco");
        result.Schedules["2026-04-15"].Select(x => x.Source).Should().Contain("Institution");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetSourceCorrectlyAsync()
    {
        List<KamcoAuctionItem> kamcoItems = [CreateKamcoEntity(1, "Kamco Item", "20260415090000", "20260416090000")];
        _kamcoRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(kamcoItems);

        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        CalendarScheduleItem item = result.Schedules["2026-04-15"].Single();
        item.Source.Should().Be("Kamco");
        item.Name.Should().Be("Kamco Item");
        item.MinBidPrice.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteAsync_InstitutionItem_ShouldHaveNullMinBidPriceAsync()
    {
        List<InstitutionAuctionItem> items = [CreateInstitutionEntity(1, "Inst", "20260415090000", "20260416090000")];
        _institutionRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(items);

        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Schedules["2026-04-15"].Single().MinBidPrice.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task ExecuteAsync_WithInvalidMonth_ShouldThrowAsync(int month)
    {
        Func<Task> act = () => _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, month));

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public async Task ExecuteAsync_WithInvalidYear_ShouldThrowAsync(int year)
    {
        Func<Task> act = () => _sut.ExecuteAsync(new GetCalendarSchedulesRequest(year, 4));

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldQueryCorrectUtcRangeAsync()
    {
        await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        // April 2026 KST: 2026-04-01 00:00 KST = 2026-03-31 15:00 UTC
        // May 2026 KST:   2026-05-01 00:00 KST = 2026-04-30 15:00 UTC
        DateTimeOffset expectedFrom = new(2026, 3, 31, 15, 0, 0, TimeSpan.Zero);
        DateTimeOffset expectedTo = new(2026, 4, 30, 15, 0, 0, TimeSpan.Zero);

        await _kamcoRepo.Received(1).GetByDateRangeAsync(
            expectedFrom,
            expectedTo,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ItemsOnDifferentDays_ShouldGroupSeparatelyAsync()
    {
        List<KamcoAuctionItem> kamcoItems =
        [
            CreateKamcoEntity(1, "Day10", "20260410090000", "20260411090000"),
            CreateKamcoEntity(2, "Day15", "20260415090000", "20260416090000"),
        ];
        _kamcoRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(kamcoItems);

        CalendarScheduleResponse result = await _sut.ExecuteAsync(new GetCalendarSchedulesRequest(2026, 4));

        result.Schedules.Should().HaveCount(2);
        result.Schedules["2026-04-10"].Should().HaveCount(1);
        result.Schedules["2026-04-15"].Should().HaveCount(1);
    }

    private static KamcoAuctionItem CreateKamcoEntity(
        long plnmNo,
        string cltrNm,
        string pbctBegnDtm,
        string pbctClsDtm)
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
            pbctBegnDtm,
            pbctClsDtm,
            0,
            0,
            null);
    }

    private static InstitutionAuctionItem CreateInstitutionEntity(
        long plnmNo,
        string plnmNm,
        string pbctBegnDtm,
        string pbctClsDtm)
    {
        return InstitutionAuctionItem.Create(
            plnmNo,
            1,
            "01",
            "Kind",
            "01",
            "BidDvsn",
            plnmNm,
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
            pbctBegnDtm,
            pbctClsDtm,
            "20260103090000",
            "CTG01",
            "Category");
    }

    private void SetupEmptyDefaults()
    {
        _kamcoRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(Array.Empty<KamcoAuctionItem>());
        _institutionRepo.GetByDateRangeAsync(
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>()).Returns(Array.Empty<InstitutionAuctionItem>());
    }
}
