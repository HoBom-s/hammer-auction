using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Infrastructure.Kafka;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="OnbidCodeInfoMessage"/> deserialization from Kafka JSON.
/// </summary>
public sealed class OnbidCodeInfoMessageDeserializationTests
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
                "ctgrId": "CTG001",
                "ctgrNm": "토지",
                "ctgrHirkId": "ROOT",
                "ctgrHirkNm": "전체"
            }
            """;

        OnbidCodeInfoMessage? msg = JsonSerializer.Deserialize<OnbidCodeInfoMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.CtgrId.Should().Be("CTG001");
        msg.CtgrNm.Should().Be("토지");
        msg.CtgrHirkId.Should().Be("ROOT");
        msg.CtgrHirkNm.Should().Be("전체");
    }

    [Fact]
    public void Roundtrip_SerializeAndDeserialize_ShouldPreserveData()
    {
        OnbidCodeInfoMessage original = new(
            CtgrId: "CTG001",
            CtgrNm: "토지",
            CtgrHirkId: "ROOT",
            CtgrHirkNm: "전체");

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        OnbidCodeInfoMessage? deserialized = JsonSerializer.Deserialize<OnbidCodeInfoMessage>(json, _jsonOptions);

        deserialized.Should().BeEquivalentTo(original);
    }
}
