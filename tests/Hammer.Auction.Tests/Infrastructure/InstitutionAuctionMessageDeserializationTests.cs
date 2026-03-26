using System.Text.Json;
using FluentAssertions;
using Hammer.Auction.Infrastructure.Kafka;

namespace Hammer.Auction.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="InstitutionAuctionMessage"/> deserialization from Kafka JSON.
/// </summary>
public sealed class InstitutionAuctionMessageDeserializationTests
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
                "plnmKindCd": "01",
                "plnmKindNm": "공매공고",
                "bidDvsnCd": "02",
                "bidDvsnNm": "전자입찰",
                "plnmNm": "서울시 강남구 소재 토지 공매",
                "orgNm": "서울특별시",
                "plnmDt": "20260325",
                "orgPlnmNo": "ORG-2026-001",
                "plnmMnmtNo": "MNMT-001",
                "bidMtdCd": "03",
                "bidMtdNm": "일반경쟁",
                "totAmtUnpcDvsnCd": "01",
                "totAmtUnpcDvsnNm": "총액",
                "dpslMtdCd": "01",
                "dpslMtdNm": "매각",
                "prptDvsnCd": "01",
                "prptDvsnNm": "토지",
                "pbctBegnDtm": "20260325100000",
                "pbctClsDtm": "20260327170000",
                "pbctExctDtm": "20260328100000",
                "ctgrId": "CTG001",
                "ctgrFullNm": "토지 / 대지"
            }
            """;

        InstitutionAuctionMessage? msg = JsonSerializer.Deserialize<InstitutionAuctionMessage>(json, _jsonOptions);

        msg.Should().NotBeNull();
        msg!.PlnmNo.Should().Be(12345);
        msg.PbctNo.Should().Be(67890);
        msg.PlnmKindCd.Should().Be("01");
        msg.PlnmKindNm.Should().Be("공매공고");
        msg.BidDvsnCd.Should().Be("02");
        msg.BidDvsnNm.Should().Be("전자입찰");
        msg.PlnmNm.Should().Be("서울시 강남구 소재 토지 공매");
        msg.OrgNm.Should().Be("서울특별시");
        msg.PlnmDt.Should().Be("20260325");
        msg.OrgPlnmNo.Should().Be("ORG-2026-001");
        msg.PlnmMnmtNo.Should().Be("MNMT-001");
        msg.BidMtdCd.Should().Be("03");
        msg.BidMtdNm.Should().Be("일반경쟁");
        msg.TotAmtUnpcDvsnCd.Should().Be("01");
        msg.TotAmtUnpcDvsnNm.Should().Be("총액");
        msg.DpslMtdCd.Should().Be("01");
        msg.DpslMtdNm.Should().Be("매각");
        msg.PrptDvsnCd.Should().Be("01");
        msg.PrptDvsnNm.Should().Be("토지");
        msg.PbctBegnDtm.Should().Be("20260325100000");
        msg.PbctClsDtm.Should().Be("20260327170000");
        msg.PbctExctDtm.Should().Be("20260328100000");
        msg.CtgrId.Should().Be("CTG001");
        msg.CtgrFullNm.Should().Be("토지 / 대지");
    }

    [Fact]
    public void Roundtrip_SerializeAndDeserialize_ShouldPreserveData()
    {
        InstitutionAuctionMessage original = new(
            PlnmNo: 100,
            PbctNo: 200,
            PlnmKindCd: "01",
            PlnmKindNm: "공매공고",
            BidDvsnCd: "02",
            BidDvsnNm: "전자입찰",
            PlnmNm: "Test Item",
            OrgNm: "Organization",
            PlnmDt: "20260301",
            OrgPlnmNo: "ORG-001",
            PlnmMnmtNo: "MNMT-001",
            BidMtdCd: "03",
            BidMtdNm: "일반경쟁",
            TotAmtUnpcDvsnCd: "01",
            TotAmtUnpcDvsnNm: "총액",
            DpslMtdCd: "01",
            DpslMtdNm: "매각",
            PrptDvsnCd: "01",
            PrptDvsnNm: "토지",
            PbctBegnDtm: "20260301090000",
            PbctClsDtm: "20260310170000",
            PbctExctDtm: "20260311100000",
            CtgrId: "CTG001",
            CtgrFullNm: "토지 / 대지");

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        InstitutionAuctionMessage? deserialized = JsonSerializer.Deserialize<InstitutionAuctionMessage>(json, _jsonOptions);

        deserialized.Should().BeEquivalentTo(original);
    }
}
