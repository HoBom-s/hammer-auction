using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Infrastructure.Kafka;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="RealEstateTradeMessage"/> deserialization from Kafka JSON.
/// </summary>
public sealed class RealEstateTradeMessageDeserializationTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public void Deserialize_WithCamelCaseJson_ShouldMapAllFields()
    {
        var json = """
            {
                "lawdCd": "11110",
                "propertyType": 1,
                "buildingName": "래미안",
                "jibun": "123-4",
                "umdNm": "종로동",
                "dealAmount": 85000,
                "dealYear": 2025,
                "dealMonth": 3,
                "dealDay": 15,
                "area": 84.99,
                "floor": 10,
                "buildYear": 2020
            }
            """;

        RealEstateTradeMessage? msg = JsonSerializer.Deserialize<RealEstateTradeMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.LawdCd.Should().Be("11110");
        msg.PropertyType.Should().Be(1);
        msg.BuildingName.Should().Be("래미안");
        msg.Jibun.Should().Be("123-4");
        msg.UmdNm.Should().Be("종로동");
        msg.DealAmount.Should().Be(85000);
        msg.DealYear.Should().Be(2025);
        msg.DealMonth.Should().Be(3);
        msg.DealDay.Should().Be(15);
        msg.Area.Should().Be(84.99m);
        msg.Floor.Should().Be(10);
        msg.BuildYear.Should().Be(2020);
    }

    [Fact]
    public void Deserialize_WithNullOptionalFields_ShouldMapCorrectly()
    {
        var json = """
            {
                "lawdCd": "11110",
                "propertyType": 5,
                "buildingName": null,
                "jibun": "456-7",
                "umdNm": "사직동",
                "dealAmount": 120000,
                "dealYear": 2025,
                "dealMonth": 1,
                "dealDay": 20,
                "area": 330.5,
                "floor": null,
                "buildYear": null
            }
            """;

        RealEstateTradeMessage? msg = JsonSerializer.Deserialize<RealEstateTradeMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.BuildingName.Should().BeNull();
        msg.Floor.Should().BeNull();
        msg.BuildYear.Should().BeNull();
    }

    [Fact]
    public void Roundtrip_SerializeAndDeserialize_ShouldPreserveData()
    {
        RealEstateTradeMessage original = new(
            LawdCd: "11110",
            PropertyType: 1,
            BuildingName: "래미안",
            Jibun: "123-4",
            UmdNm: "종로동",
            DealAmount: 85000,
            DealYear: 2025,
            DealMonth: 3,
            DealDay: 15,
            Area: 84.99m,
            Floor: 10,
            BuildYear: 2020);

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        RealEstateTradeMessage? deserialized = JsonSerializer.Deserialize<RealEstateTradeMessage>(json, _jsonOptions);

        deserialized.Should().BeEquivalentTo(original);
    }
}
