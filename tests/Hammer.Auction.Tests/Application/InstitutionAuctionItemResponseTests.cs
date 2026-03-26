using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="InstitutionAuctionItemResponse"/>.
/// </summary>
public sealed class InstitutionAuctionItemResponseTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFields()
    {
        var entity = InstitutionAuctionItem.Create(
            100,
            200,
            "01",
            "공개경쟁",
            "01",
            "일반입찰",
            "테스트 공고",
            "한국자산관리공사",
            "20260315",
            "ORG-001",
            "MNMT-001",
            "01",
            "전자입찰",
            "01",
            "총액",
            "01",
            "매각",
            "01",
            "국유재산",
            "20260315100000",
            "20260320170000",
            "20260321100000",
            "CTG01",
            "토지 > 대지");

        var response = InstitutionAuctionItemResponse.FromEntity(entity);

        response.PlnmNo.Should().Be(100);
        response.PbctNo.Should().Be(200);
        response.PlnmKindCd.Should().Be("01");
        response.PlnmKindNm.Should().Be("공개경쟁");
        response.BidDvsnCd.Should().Be("01");
        response.BidDvsnNm.Should().Be("일반입찰");
        response.PlnmNm.Should().Be("테스트 공고");
        response.OrgNm.Should().Be("한국자산관리공사");
        response.PlnmDt.Should().Be("20260315");
        response.OrgPlnmNo.Should().Be("ORG-001");
        response.PlnmMnmtNo.Should().Be("MNMT-001");
        response.BidMtdCd.Should().Be("01");
        response.BidMtdNm.Should().Be("전자입찰");
        response.TotAmtUnpcDvsnCd.Should().Be("01");
        response.TotAmtUnpcDvsnNm.Should().Be("총액");
        response.DpslMtdCd.Should().Be("01");
        response.DpslMtdNm.Should().Be("매각");
        response.PrptDvsnCd.Should().Be("01");
        response.PrptDvsnNm.Should().Be("국유재산");
        response.CtgrId.Should().Be("CTG01");
        response.CtgrFullNm.Should().Be("토지 > 대지");
        response.PbctBegnDtm.Day.Should().Be(15);
        response.PbctClsDtm.Day.Should().Be(20);
        response.PbctExctDtm.Day.Should().Be(21);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FromEntity_WithNullEntity_ShouldThrow()
    {
        Action act = () => InstitutionAuctionItemResponse.FromEntity(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
