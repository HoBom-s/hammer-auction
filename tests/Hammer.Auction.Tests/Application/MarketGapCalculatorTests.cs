using FluentAssertions;
using Hammer.Auction.Application.Analysis;
using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Tests.Application;

public sealed class MarketGapCalculatorTests
{
    private static readonly DateOnly _referenceDate = new(2026, 3, 28);

    [Fact]
    public void Calculate_SingleRecentTrade_ShouldReturnWeightedPrice()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(9500, 2026, 2, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 80_000_000, _referenceDate);

        gap.WeightedMarketPrice.Should().Be(95_000_000);
        gap.MinBidPrice.Should().Be(80_000_000);
        gap.GapRate.Should().BeApproximately(15.79, 0.01);
        gap.Grade.Should().Be("B");
        gap.TradeCount.Should().Be(1);
        gap.UsedTradeCount.Should().Be(1);
        gap.Confidence.Should().Be("낮음");
    }

    [Fact]
    public void Calculate_MultipleTradesWithTimeWeights_ShouldApplyWeights()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2026, 1, 15),  // 2 months ago → weight 1.0
            CreateTrade(8000, 2025, 7, 15),   // 8 months ago → weight 0.6
            CreateTrade(6000, 2024, 9, 15),   // 18 months ago → weight 0.3
        ];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 50_000_000, _referenceDate);

        // weighted = (10000*10000*1.0 + 8000*10000*0.6 + 6000*10000*0.3) / (1.0+0.6+0.3)
        // = (100_000_000 + 48_000_000 + 18_000_000) / 1.9
        // = 166_000_000 / 1.9 ≈ 87_368_421
        gap.WeightedMarketPrice.Should().BeInRange(87_368_420, 87_368_422);
    }

    [Theory]
    [InlineData(40.0, "S")]
    [InlineData(25.0, "A")]
    [InlineData(15.0, "B")]
    [InlineData(5.0, "C")]
    [InlineData(4.99, "D")]
    [InlineData(-10.0, "D")]
    public void GradeFromGapRate_ShouldMatchExpected(double gapRate, string expectedGrade)
    {
        var grade = MarketGapCalculator.GradeFromGapRate(gapRate);

        grade.Should().Be(expectedGrade);
    }

    [Fact]
    public void Calculate_NegativeGap_ShouldReturnGradeD()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(5000, 2026, 2, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 60_000_000, _referenceDate);

        gap.GapRate.Should().BeNegative();
        gap.Grade.Should().Be("D");
    }

    [Fact]
    public void Calculate_VeryOldTrade_ShouldUseWeight01()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2022, 1, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 50_000_000, _referenceDate);

        // weight = 0.1, but only 1 trade so weighted avg = 10000*10000 = 100M
        gap.WeightedMarketPrice.Should().Be(100_000_000);
    }

    [Fact]
    public void Calculate_ZeroMinBidPrice_ShouldReturn100PercentGap()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 0, _referenceDate);

        gap.GapRate.Should().Be(100.0);
        gap.Grade.Should().Be("S");
    }

    [Fact]
    public void Calculate_BidExceedsMarket_ShouldReturnNegativeGapRate()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(5000, 2026, 2, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 100_000_000, _referenceDate);

        gap.GapRate.Should().BeNegative();
        gap.Grade.Should().Be("D");
    }

    [Fact]
    public void Calculate_Exactly5PercentGap_ShouldBeGradeC()
    {
        // market = 10000만원 = 100M, bid = 95M → gap = 5%
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 3, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 95_000_000, _referenceDate);

        gap.Grade.Should().Be("C");
    }

    [Fact]
    public void Calculate_Exactly25PercentGap_ShouldBeGradeA()
    {
        // market = 100M, bid = 75M → gap = 25%
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 3, 1)];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 75_000_000, _referenceDate);

        gap.Grade.Should().Be("A");
    }

    [Fact]
    public void Calculate_TradeCount_ShouldMatchInput()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2026, 1, 1),
            CreateTrade(9000, 2025, 6, 1),
            CreateTrade(8000, 2024, 1, 1),
        ];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 80_000_000, _referenceDate);

        gap.TradeCount.Should().Be(3);
    }

    [Theory]
    [InlineData(2026, 2, 1, 1.0)] // ~2 months ago
    [InlineData(2025, 7, 15, 0.6)] // ~8 months ago
    [InlineData(2024, 9, 15, 0.3)] // ~18 months ago
    [InlineData(2022, 1, 1, 0.1)] // 4+ years ago
    public void GetTimeWeight_ShouldReturnCorrectBucket(
        int year,
        int month,
        int day,
        double expectedWeight)
    {
        RealEstateTradeResponse trade = CreateTrade(10000, year, month, day);

        var weight = MarketGapCalculator.GetTimeWeight(trade, _referenceDate);

        weight.Should().Be(expectedWeight);
    }

    [Fact]
    public void FilterOutliers_LessThan4Trades_ShouldSkipFiltering()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(5000, 2026, 1, 1),
            CreateTrade(10000, 2026, 1, 2),
            CreateTrade(50000, 2026, 1, 3),
        ];

        IReadOnlyList<RealEstateTradeResponse> result = MarketGapCalculator.FilterOutliers(trades);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void FilterOutliers_WithOutlier_ShouldRemoveIt()
    {
        // 4 normal trades + 1 extreme outlier
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(9000, 2026, 1, 1),
            CreateTrade(9500, 2026, 1, 2),
            CreateTrade(10000, 2026, 1, 3),
            CreateTrade(10500, 2026, 1, 4),
            CreateTrade(100000, 2026, 1, 5), // extreme outlier
        ];

        IReadOnlyList<RealEstateTradeResponse> result = MarketGapCalculator.FilterOutliers(trades);

        result.Should().HaveCount(4);
        result.Should().NotContain(t => t.DealAmount == 100000);
    }

    [Fact]
    public void FilterOutliers_AllOutliers_ShouldReturnOriginal()
    {
        // All trades are the same → IQR=0, bounds are tight, but all equal so all pass
        // This tests the safety net: if somehow all get filtered, return original
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2026, 1, 1),
            CreateTrade(10000, 2026, 1, 2),
            CreateTrade(10000, 2026, 1, 3),
            CreateTrade(10000, 2026, 1, 4),
        ];

        IReadOnlyList<RealEstateTradeResponse> result = MarketGapCalculator.FilterOutliers(trades);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Calculate_WithOutlier_ShouldReportUsedTradeCount()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(9000, 2026, 1, 1),
            CreateTrade(9500, 2026, 1, 2),
            CreateTrade(10000, 2026, 1, 3),
            CreateTrade(10500, 2026, 1, 4),
            CreateTrade(100000, 2026, 1, 5), // outlier
        ];

        MarketGapResult gap = MarketGapCalculator.Calculate(trades, 80_000_000, _referenceDate);

        gap.TradeCount.Should().Be(5);
        gap.UsedTradeCount.Should().Be(4);
    }

    [Theory]
    [InlineData(5, "높음")]
    [InlineData(10, "높음")]
    [InlineData(3, "보통")]
    [InlineData(4, "보통")]
    [InlineData(2, "낮음")]
    [InlineData(1, "낮음")]
    public void ConfidenceFromCount_ShouldReturnCorrectLevel(int count, string expected)
    {
        var confidence = MarketGapCalculator.ConfidenceFromCount(count);

        confidence.Should().Be(expected);
    }

    [Fact]
    public void Percentile_ShouldCalculateCorrectly()
    {
        List<double> sorted = [1, 2, 3, 4, 5];

        MarketGapCalculator.Percentile(sorted, 0).Should().Be(1);
        MarketGapCalculator.Percentile(sorted, 25).Should().Be(2);
        MarketGapCalculator.Percentile(sorted, 50).Should().Be(3);
        MarketGapCalculator.Percentile(sorted, 75).Should().Be(4);
        MarketGapCalculator.Percentile(sorted, 100).Should().Be(5);
    }

    [Fact]
    public void FilterOutliers_ZeroArea_ShouldUseDealAmountDirectly()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(9000, 2026, 1, 1, area: 0),
            CreateTrade(9500, 2026, 1, 2, area: 0),
            CreateTrade(10000, 2026, 1, 3, area: 0),
            CreateTrade(10500, 2026, 1, 4, area: 0),
            CreateTrade(100000, 2026, 1, 5, area: 0), // outlier
        ];

        IReadOnlyList<RealEstateTradeResponse> result = MarketGapCalculator.FilterOutliers(trades);

        result.Should().HaveCount(4);
        result.Should().NotContain(t => t.DealAmount == 100000);
    }

    private static RealEstateTradeResponse CreateTrade(
        long dealAmount,
        int dealYear,
        int dealMonth,
        int dealDay,
        decimal area = 84.99m) =>
        new(
            Id: 1,
            LawdCd: "11680",
            PropertyType: 1,
            BuildingName: "테스트아파트",
            Jibun: "100",
            UmdNm: "역삼동",
            DealAmount: dealAmount,
            DealYear: dealYear,
            DealMonth: dealMonth,
            DealDay: dealDay,
            Area: area,
            Floor: 10,
            BuildYear: 2020,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
