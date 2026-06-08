using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="RealEstateTradeResponse"/>.
/// </summary>
public sealed class RealEstateTradeResponseTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFields()
    {
        var entity = RealEstateTrade.Create(
            "11110",
            1,
            "래미안",
            "123-4",
            "종로동",
            85000,
            2025,
            3,
            15,
            84.99m,
            10,
            2020);

        var response = RealEstateTradeResponse.FromEntity(entity);

        response.LawdCd.Should().Be("11110");
        response.PropertyType.Should().Be(1);
        response.BuildingName.Should().Be("래미안");
        response.Jibun.Should().Be("123-4");
        response.UmdNm.Should().Be("종로동");
        response.DealAmount.Should().Be(85000);
        response.DealYear.Should().Be(2025);
        response.DealMonth.Should().Be(3);
        response.DealDay.Should().Be(15);
        response.Area.Should().Be(84.99m);
        response.Floor.Should().Be(10);
        response.BuildYear.Should().Be(2020);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FromEntity_WithNullEntity_ShouldThrow()
    {
        Action act = () => RealEstateTradeResponse.FromEntity(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromEntity_WithNullOptionalFields_ShouldMapCorrectly()
    {
        var entity = RealEstateTrade.Create(
            "11110",
            5,
            null,
            "456-7",
            "사직동",
            120000,
            2025,
            1,
            20,
            330.5m,
            null,
            null);

        var response = RealEstateTradeResponse.FromEntity(entity);

        response.BuildingName.Should().BeNull();
        response.Floor.Should().BeNull();
        response.BuildYear.Should().BeNull();
    }
}
