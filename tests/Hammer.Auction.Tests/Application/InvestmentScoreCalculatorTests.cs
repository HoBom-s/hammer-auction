using FluentAssertions;
using Hammer.Auction.Application.Analysis;
using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Tests.Application;

public sealed class InvestmentScoreCalculatorTests
{
    private static readonly DateOnly _referenceDate = new(2026, 3, 28);

    [Fact]
    public void Calculate_ShouldSumAllComponents()
    {
        var marketGap = new MarketGapResult(100_000_000, 80_000_000, 20.0, "B", 1, 1, "낮음");
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentScoreResult score = InvestmentScoreCalculator.Calculate(
            marketGap, trades, 2, 30, _referenceDate);

        var expectedTotal = score.MarketGapScore + score.PriceTrendScore
            + score.DiscountDepthScore + score.AppraisalDiscountScore
            + score.CompetitionScore + score.LiquidityScore;
        score.TotalScore.Should().BeApproximately(expectedTotal, 0.1);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(4, 20)]
    [InlineData(10, 20)]
    public void Calculate_DiscountDepthScore_ShouldCapAt20(int uscbdCnt, double expected)
    {
        var marketGap = new MarketGapResult(100_000_000, 80_000_000, 20.0, "B", 1, 1, "낮음");
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentScoreResult score = InvestmentScoreCalculator.Calculate(
            marketGap, trades, uscbdCnt, 0, _referenceDate);

        score.DiscountDepthScore.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(49, 10)]
    [InlineData(50, 8)]
    [InlineData(99, 8)]
    [InlineData(100, 6)]
    [InlineData(199, 6)]
    [InlineData(200, 4)]
    [InlineData(499, 4)]
    [InlineData(500, 2)]
    [InlineData(999, 2)]
    [InlineData(1000, 1)]
    [InlineData(5000, 1)]
    public void CompetitionScoreFromInquiry_ShouldBeInverseOfInquiry(int iqryCnt, double expected)
    {
        var score = InvestmentScoreCalculator.CompetitionScoreFromInquiry(iqryCnt);

        score.Should().Be(expected);
    }

    [Fact]
    public void CalculateLiquidityScore_ShouldCountOnly12MonthTrades()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2026, 2, 1),
            CreateTrade(10000, 2025, 6, 1),
            CreateTrade(10000, 2024, 1, 1),
        ];

        var score = InvestmentScoreCalculator.CalculateLiquidityScore(trades, _referenceDate);

        // 2 trades within 12 months → min(2*2, 10) = 4
        score.Should().Be(4);
    }

    [Fact]
    public void CalculateLiquidityScore_ShouldCapAt10()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2026, 3, 1),
            CreateTrade(10000, 2026, 2, 1),
            CreateTrade(10000, 2026, 1, 1),
            CreateTrade(10000, 2025, 12, 1),
            CreateTrade(10000, 2025, 11, 1),
            CreateTrade(10000, 2025, 10, 1),
        ];

        var score = InvestmentScoreCalculator.CalculateLiquidityScore(trades, _referenceDate);

        // 6 trades within 12 months → min(6*2, 10) = 10
        score.Should().Be(10);
    }

    [Fact]
    public void CalculateLiquidityScore_AllOldTrades_ShouldBeZero()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(10000, 2024, 1, 1),
            CreateTrade(10000, 2023, 6, 1),
        ];

        var score = InvestmentScoreCalculator.CalculateLiquidityScore(trades, _referenceDate);

        score.Should().Be(0);
    }

    [Fact]
    public void CalculatePriceTrendScore_WhenBothPeriodsHaveData_ShouldCalculate()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(11000, 2026, 1, 1),
            CreateTrade(10000, 2025, 7, 1),
        ];

        var score = InvestmentScoreCalculator.CalculatePriceTrendScore(trades, _referenceDate);

        // change = (11000-10000)/10000 = 0.10 → score = 10 + 0.10/0.20*10 = 15.0
        score.Should().BeApproximately(15.0, 0.1);
    }

    [Fact]
    public void CalculatePriceTrendScore_WhenOnePeriodEmpty_ShouldReturnNeutral()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        var score = InvestmentScoreCalculator.CalculatePriceTrendScore(trades, _referenceDate);

        score.Should().Be(10.0);
    }

    [Fact]
    public void CalculatePriceTrendScore_Declining_ShouldReturnLowScore()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(8000, 2026, 1, 1),   // recent: lower
            CreateTrade(10000, 2025, 7, 1),  // previous: higher
        ];

        var score = InvestmentScoreCalculator.CalculatePriceTrendScore(trades, _referenceDate);

        // change = (8000-10000)/10000 = -0.20 → score = 10 + (-1.0)*10 = 0
        score.Should().BeApproximately(0.0, 0.1);
    }

    [Fact]
    public void CalculatePriceTrendScore_StrongIncrease_ShouldCapAt20()
    {
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(20000, 2026, 1, 1),  // recent: doubled
            CreateTrade(10000, 2025, 7, 1),  // previous
        ];

        var score = InvestmentScoreCalculator.CalculatePriceTrendScore(trades, _referenceDate);

        // change = (20000-10000)/10000 = 1.0 → unclamped = 10 + 5*10 = 60 → clamped to 20
        score.Should().Be(20);
    }

    [Fact]
    public void RatingFromTotalScore_ShouldReturnValidValues()
    {
        InvestmentScoreCalculator.RatingFromTotalScore(80).Should().Be("최상");
        InvestmentScoreCalculator.RatingFromTotalScore(60).Should().Be("상");
        InvestmentScoreCalculator.RatingFromTotalScore(40).Should().Be("중");
        InvestmentScoreCalculator.RatingFromTotalScore(20).Should().Be("하");
        InvestmentScoreCalculator.RatingFromTotalScore(10).Should().Be("최하");
    }

    [Theory]
    [InlineData("S", 25)]
    [InlineData("A", 20)]
    [InlineData("B", 15)]
    [InlineData("C", 10)]
    [InlineData("D", 5)]
    public void MarketGapScoreFromGrade_ShouldMapCorrectly(string grade, double expected)
    {
        var score = InvestmentScoreCalculator.MarketGapScoreFromGrade(grade);

        score.Should().Be(expected);
    }

    [Fact]
    public void Calculate_GradeS_ShouldGiveHighTotalScore()
    {
        var marketGap = new MarketGapResult(100_000_000, 10_000_000, 90.0, "S", 1, 1, "낮음");
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentScoreResult score = InvestmentScoreCalculator.Calculate(
            marketGap, trades, 3, 10, _referenceDate);

        // S grade = 25 + 10(neutral) + 15(3*5) + 7.5(no appraisal) + 10(iqry<50) + 2(1 trade) = 69.5
        score.MarketGapScore.Should().Be(25);
        score.TotalScore.Should().BeGreaterThan(60);
    }

    [Fact]
    public void Calculate_TotalScore_ShouldNotExceed100()
    {
        var marketGap = new MarketGapResult(100_000_000, 10_000_000, 90.0, "S", 6, 6, "높음");
        List<RealEstateTradeResponse> trades =
        [
            CreateTrade(20000, 2026, 2, 1),
            CreateTrade(10000, 2025, 7, 1),
            CreateTrade(10000, 2026, 1, 1),
            CreateTrade(10000, 2025, 12, 1),
            CreateTrade(10000, 2025, 11, 1),
            CreateTrade(10000, 2025, 10, 1),
        ];

        InvestmentScoreResult score = InvestmentScoreCalculator.Calculate(
            marketGap, trades, 5, 0, _referenceDate, 200_000_000);

        score.TotalScore.Should().BeLessThanOrEqualTo(100);
    }

    [Theory]
    [InlineData(100_000_000, 50_000_000, 15)] // 50% discount → 15
    [InlineData(100_000_000, 65_000_000, 12)] // 35% discount → 12
    [InlineData(100_000_000, 80_000_000, 9)] // 20% discount → 9
    [InlineData(100_000_000, 90_000_000, 6)] // 10% discount → 6
    [InlineData(100_000_000, 95_000_000, 3)] // 5% discount → 3
    public void CalculateAppraisalDiscountScore_ShouldMatchDiscount(
        long appraisalAmount,
        long minBidPrice,
        double expected)
    {
        var score = InvestmentScoreCalculator.CalculateAppraisalDiscountScore(
            appraisalAmount, minBidPrice);

        score.Should().Be(expected);
    }

    [Fact]
    public void CalculateAppraisalDiscountScore_ZeroAppraisal_ShouldReturnNeutral()
    {
        var score = InvestmentScoreCalculator.CalculateAppraisalDiscountScore(0, 80_000_000);

        score.Should().Be(7.5);
    }

    [Fact]
    public void CalculateAppraisalDiscountScore_NegativeAppraisal_ShouldReturnNeutral()
    {
        var score = InvestmentScoreCalculator.CalculateAppraisalDiscountScore(-1, 80_000_000);

        score.Should().Be(7.5);
    }

    [Fact]
    public void Calculate_WithAppraisalAmount_ShouldIncludeAppraisalScore()
    {
        var marketGap = new MarketGapResult(100_000_000, 60_000_000, 40.0, "S", 1, 1, "낮음");
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentScoreResult score = InvestmentScoreCalculator.Calculate(
            marketGap, trades, 2, 30, _referenceDate, 100_000_000);

        // minBidPrice=60M, appraisal=100M → 40% discount → 12 points
        score.AppraisalDiscountScore.Should().Be(12);
    }

    private static RealEstateTradeResponse CreateTrade(
        long dealAmount,
        int dealYear,
        int dealMonth,
        int dealDay) =>
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
            Area: 84.99m,
            Floor: 10,
            BuildYear: 2020,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
