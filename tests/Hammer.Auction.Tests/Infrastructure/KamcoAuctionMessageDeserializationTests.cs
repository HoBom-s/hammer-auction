using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Infrastructure.Kafka;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="KamcoAuctionMessage"/> deserialization from Kafka JSON.
/// </summary>
public sealed class KamcoAuctionMessageDeserializationTests
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
                "plnmNo": 12345,
                "pbctNo": 67890,
                "cltrNo": 11111,
                "cltrNm": "서울 강남구 역삼동",
                "ctgrFullNm": "토지 / 대지",
                "ldnmAdrs": "서울 강남구 역삼동 123",
                "nmrdAdrs": "서울 강남구 테헤란로 1",
                "minBidPrc": 500000000,
                "apslAsesAvgAmt": 700000000,
                "bidMtdNm": "일반경쟁",
                "pbctCltrStatNm": "입찰진행중",
                "pbctBegnDtm": "20260325100000",
                "pbctClsDtm": "20260327170000",
                "uscbdCnt": 3,
                "iqryCnt": 42,
                "cltrImgFiles": null
            }
            """;

        KamcoAuctionMessage? msg = JsonSerializer.Deserialize<KamcoAuctionMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.PlnmNo.Should().Be(12345);
        msg.PbctNo.Should().Be(67890);
        msg.CltrNo.Should().Be(11111);
        msg.CltrNm.Should().Be("서울 강남구 역삼동");
        msg.CtgrFullNm.Should().Be("토지 / 대지");
        msg.LdnmAdrs.Should().Be("서울 강남구 역삼동 123");
        msg.NmrdAdrs.Should().Be("서울 강남구 테헤란로 1");
        msg.MinBidPrc.Should().Be(500_000_000);
        msg.ApslAsesAvgAmt.Should().Be(700_000_000);
        msg.BidMtdNm.Should().Be("일반경쟁");
        msg.PbctCltrStatNm.Should().Be("입찰진행중");
        msg.PbctBegnDtm.Should().Be("20260325100000");
        msg.PbctClsDtm.Should().Be("20260327170000");
        msg.UscbdCnt.Should().Be(3);
        msg.IqryCnt.Should().Be(42);
        msg.CltrImgFiles.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithImageUrl_ShouldDeserialize()
    {
        var json = """
            {
                "plnmNo": 1,
                "pbctNo": 2,
                "cltrNo": 3,
                "cltrNm": "Test",
                "ctgrFullNm": "Cat",
                "ldnmAdrs": "Addr1",
                "nmrdAdrs": "Addr2",
                "minBidPrc": 100,
                "apslAsesAvgAmt": 200,
                "bidMtdNm": "Method",
                "pbctCltrStatNm": "Status",
                "pbctBegnDtm": "20260101090000",
                "pbctClsDtm": "20260102090000",
                "uscbdCnt": 0,
                "iqryCnt": 0,
                "cltrImgFiles": ["https://example.com/image.jpg", "https://example.com/image2.jpg"]
            }
            """;

        KamcoAuctionMessage? msg = JsonSerializer.Deserialize<KamcoAuctionMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.CltrImgFiles.Should().HaveCount(2);
        msg.CltrImgFiles![0].Should().Be("https://example.com/image.jpg");
        msg.CltrImgFiles[1].Should().Be("https://example.com/image2.jpg");
    }

    [Fact]
    public void Roundtrip_SerializeAndDeserialize_ShouldPreserveData()
    {
        KamcoAuctionMessage original = new(
            PlnmNo: 100,
            PbctNo: 200,
            CltrNo: 300,
            CltrNm: "Test Item",
            CtgrFullNm: "Category",
            LdnmAdrs: "Address 1",
            NmrdAdrs: "Address 2",
            MinBidPrc: 500,
            ApslAsesAvgAmt: 1000,
            BidMtdNm: "Method",
            PbctCltrStatNm: "Active",
            PbctBegnDtm: "20260301090000",
            PbctClsDtm: "20260310170000",
            UscbdCnt: 2,
            IqryCnt: 50,
            CltrImgFiles: null);

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        KamcoAuctionMessage? deserialized = JsonSerializer.Deserialize<KamcoAuctionMessage>(json, _jsonOptions);

        deserialized.Should().BeEquivalentTo(original);
    }
}
