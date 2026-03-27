using FluentAssertions;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Domain;

/// <summary>
/// Tests for <see cref="RealEstateTrade"/> domain entity.
/// </summary>
public sealed class RealEstateTradeTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var item = RealEstateTrade.Create(
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

        item.LawdCd.Should().Be("11110");
        item.PropertyType.Should().Be(1);
        item.BuildingName.Should().Be("래미안");
        item.Jibun.Should().Be("123-4");
        item.UmdNm.Should().Be("종로동");
        item.DealAmount.Should().Be(85000);
        item.DealYear.Should().Be(2025);
        item.DealMonth.Should().Be(3);
        item.DealDay.Should().Be(15);
        item.Area.Should().Be(84.99m);
        item.Floor.Should().Be(10);
        item.BuildYear.Should().Be(2020);
        item.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        item.UpdatedAt.Should().Be(item.CreatedAt);
    }

    [Fact]
    public void Create_WithNullOptionalFields_ShouldSetNull()
    {
        var item = RealEstateTrade.Create(
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

        item.BuildingName.Should().BeNull();
        item.Floor.Should().BeNull();
        item.BuildYear.Should().BeNull();
    }

    [Fact]
    public void UpdateFromSnapshot_ShouldUpdateMutableFields()
    {
        var item = RealEstateTrade.Create(
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

        DateTimeOffset originalCreatedAt = item.CreatedAt;

        item.UpdateFromSnapshot(
            "자이",
            "사직동",
            92000,
            15,
            2021);

        item.BuildingName.Should().Be("자이");
        item.UmdNm.Should().Be("사직동");
        item.DealAmount.Should().Be(92000);
        item.Floor.Should().Be(15);
        item.BuildYear.Should().Be(2021);
        item.LawdCd.Should().Be("11110");
        item.PropertyType.Should().Be(1);
        item.Jibun.Should().Be("123-4");
        item.DealYear.Should().Be(2025);
        item.DealMonth.Should().Be(3);
        item.DealDay.Should().Be(15);
        item.Area.Should().Be(84.99m);
        item.CreatedAt.Should().Be(originalCreatedAt);
        item.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public void UpdateFromSnapshot_ShouldAllowNullingOptionalFields()
    {
        var item = RealEstateTrade.Create(
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

        item.UpdateFromSnapshot(null, "종로동", 85000, null, null);

        item.BuildingName.Should().BeNull();
        item.Floor.Should().BeNull();
        item.BuildYear.Should().BeNull();
    }
}
