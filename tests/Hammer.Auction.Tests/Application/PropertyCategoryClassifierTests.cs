using FluentAssertions;
using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Tests.Application;

/// <summary>
///     Tests for <see cref="PropertyCategoryClassifier" />.
/// </summary>
public sealed class PropertyCategoryClassifierTests
{
    [Theory]
    [InlineData("토지")]
    [InlineData("토지 / 대지")]
    [InlineData("주거용건물")]
    [InlineData("주거용건물 / 아파트")]
    [InlineData("상가용및업무용건물")]
    [InlineData("상가용및업무용건물 / 오피스텔")]
    [InlineData("산업용및기타특수용건물")]
    [InlineData("용도복합용건물")]
    public void Classify_WithRealEstateSegment_ShouldReturnRealEstate(string ctgrFullNm)
    {
        PropertyCategory result = PropertyCategoryClassifier.Classify(ctgrFullNm);

        result.Should().Be(PropertyCategory.부동산);
    }

    [Theory]
    [InlineData("자동차")]
    [InlineData("자동차 / 승용차")]
    [InlineData("건축자재및기계")]
    [InlineData("선박")]
    [InlineData("시계/귀금속")]
    [InlineData("컴퓨터/전기/통신기계")]
    [InlineData("석유/화학/연료")]
    public void Classify_WithMovableSegment_ShouldReturnMovable(string ctgrFullNm)
    {
        PropertyCategory result = PropertyCategoryClassifier.Classify(ctgrFullNm);

        result.Should().Be(PropertyCategory.동산);
    }

    [Theory]
    [InlineData("무형자산")]
    [InlineData("유가증권")]
    [InlineData("회원권")]
    [InlineData("기타")]
    [InlineData("")]
    public void Classify_WithUnknownSegment_ShouldReturnOther(string ctgrFullNm)
    {
        PropertyCategory result = PropertyCategoryClassifier.Classify(ctgrFullNm);

        result.Should().Be(PropertyCategory.기타);
    }

    [Fact]
    public void Classify_WithNull_ShouldThrowArgumentNullException()
    {
        Action act = () => PropertyCategoryClassifier.Classify(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
