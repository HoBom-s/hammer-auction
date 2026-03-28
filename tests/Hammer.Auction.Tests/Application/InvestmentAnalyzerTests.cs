using FluentAssertions;
using Hammer.Auction.Application.Analysis;
using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Tests.Application;

public sealed class InvestmentAnalyzerTests
{
    private static readonly DateOnly _referenceDate = new(2026, 3, 28);

    [Fact]
    public void Analyze_WithNoTrades_ShouldReturnNull()
    {
        InvestmentAnalysis? result = InvestmentAnalyzer.Analyze(
            80_000_000, 0, 0, "주거용건물", [], _referenceDate);

        result.Should().BeNull();
    }

    [Fact]
    public void Analyze_WithTrades_ShouldReturnAllSubResults()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentAnalysis? result = InvestmentAnalyzer.Analyze(
            80_000_000, 0, 0, "주거용건물", trades, _referenceDate);

        result.Should().NotBeNull();
        result!.MarketGap.Should().NotBeNull();
        result.InvestmentScore.Should().NotBeNull();
        result.BidPriceGuide.Should().NotBeNull();
    }

    [Fact]
    public void Analyze_Smoke_ShouldProduceConsistentEndToEndResult()
    {
        List<RealEstateTradeResponse> trades = [CreateTrade(10000, 2026, 2, 1)];

        InvestmentAnalysis? result = InvestmentAnalyzer.Analyze(
            80_000_000, 2, 30, "주거용건물", trades, _referenceDate);

        // Model 1: market gap
        result!.MarketGap!.WeightedMarketPrice.Should().Be(100_000_000);
        result.MarketGap.Grade.Should().Be("B");

        // Model 2: investment score components sum to total
        InvestmentScoreResult score = result.InvestmentScore!;
        var expectedTotal = score.MarketGapScore + score.PriceTrendScore
            + score.DiscountDepthScore + score.AppraisalDiscountScore
            + score.CompetitionScore + score.LiquidityScore;
        score.TotalScore.Should().BeApproximately(expectedTotal, 0.1);

        // Model 3: bid price ordering
        result.BidPriceGuide!.ConservativeBid.Should().BeLessThan(result.BidPriceGuide.AggressiveBid);
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
