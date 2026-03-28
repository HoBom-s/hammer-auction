using FluentAssertions;
using Hammer.Auction.Application.Analysis;

namespace Hammer.Auction.Tests.Application;

public sealed class BidPriceGuideCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnThreeTierBids()
    {
        // Market price = 100M, eviction = 3% (residential)
        // tax=4.6%, eviction=3%, misc=1% → total cost=8.6%
        // Conservative: 100M × (1 - 0.20 - 0.086) = 71_400_000
        // Moderate: 100M × (1 - 0.10 - 0.086) = 81_400_000
        // Aggressive: 100M × (1 - 0.05 - 0.086) = 86_400_000
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(100_000_000, 3.0, 2);

        guide.ConservativeBid.Should().Be(71_400_000);
        guide.ModerateBid.Should().Be(81_400_000);
        guide.AggressiveBid.Should().Be(86_400_000);
        guide.AcquisitionTaxRate.Should().Be(4.6);
        guide.EvictionCostRate.Should().Be(3.0);
        guide.MiscCostRate.Should().Be(1.0);
    }

    [Fact]
    public void Calculate_ShouldOrderConservativeLessThanAggressive()
    {
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(100_000_000, 3.0, 0);

        guide.ConservativeBid.Should().BeLessThan(guide.ModerateBid);
        guide.ModerateBid.Should().BeLessThan(guide.AggressiveBid);
    }

    [Fact]
    public void Calculate_ZeroEviction_ShouldUseLowerTotalCost()
    {
        // tax=4.6%, eviction=0%, misc=1% → total=5.6%
        // Conservative: 100M × (1 - 0.20 - 0.056) = 74_400_000
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(100_000_000, 0.0, 0);

        guide.ConservativeBid.Should().Be(74_400_000);
        guide.EvictionCostRate.Should().Be(0);
    }

    [Theory]
    [InlineData("주거용건물 / 단독주택", 3.0)]
    [InlineData("주거용건물 / 아파트", 3.0)]
    [InlineData("빌라", 3.0)]
    [InlineData("연립주택", 3.0)]
    [InlineData("다세대주택", 3.0)]
    [InlineData("오피스텔", 3.0)]
    [InlineData("상가 / 근린생활시설", 2.0)]
    [InlineData("사무용 건물", 2.0)]
    [InlineData("기타건물", 1.0)]
    [InlineData("토지 / 대지", 0.0)]
    [InlineData("차량 / 승용차", 0.0)]
    public void GetEvictionCostRate_ShouldVaryByCategory(string ctgrFullNm, double expectedRate)
    {
        var rate = BidPriceGuideCalculator.GetEvictionCostRate(ctgrFullNm);

        rate.Should().Be(expectedRate);
    }

    [Fact]
    public void GetEvictionCostRate_EmptyCategory_ShouldReturnZero()
    {
        var rate = BidPriceGuideCalculator.GetEvictionCostRate(string.Empty);

        rate.Should().Be(0);
    }

    [Theory]
    [InlineData(0, "첫 입찰")]
    [InlineData(1, "1회 유찰")]
    [InlineData(2, "2회 유찰")]
    [InlineData(3, "3회 유찰")]
    [InlineData(5, "5회 유찰")]
    public void GetGuidanceText_ShouldContainFailedBidCount(int uscbdCnt, string expectedFragment)
    {
        var guidance = BidPriceGuideCalculator.GetGuidanceText(uscbdCnt);

        guidance.Should().Contain(expectedFragment);
    }

    [Fact]
    public void Calculate_WithAppraisalAmount_ShouldCalculateDiscountRate()
    {
        // appraisal=100M, minBid=70M → discount = (1 - 70/100) * 100 = 30%
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(
            100_000_000, 3.0, 2, 100_000_000, 70_000_000);

        guide.AppraisalDiscountRate.Should().Be(30.0);
    }

    [Fact]
    public void Calculate_WithZeroAppraisal_ShouldReturnZeroDiscountRate()
    {
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(100_000_000, 3.0, 0);

        guide.AppraisalDiscountRate.Should().Be(0.0);
    }

    [Fact]
    public void Calculate_WithAppraisalLessThanMinBid_ShouldReturnNegativeRate()
    {
        // appraisal=80M, minBid=100M → discount = (1 - 100/80) * 100 = -25%
        BidPriceGuideResult guide = BidPriceGuideCalculator.Calculate(
            100_000_000, 3.0, 0, 80_000_000, 100_000_000);

        guide.AppraisalDiscountRate.Should().Be(-25.0);
    }
}
