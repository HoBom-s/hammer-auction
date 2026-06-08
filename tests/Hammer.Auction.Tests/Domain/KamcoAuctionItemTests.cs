using FluentAssertions;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Domain;

/// <summary>
/// Tests for <see cref="KamcoAuctionItem"/> rich domain entity.
/// </summary>
public sealed class KamcoAuctionItemTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var item = KamcoAuctionItem.Create(
            100,
            200,
            300,
            "서울 강남구 역삼동",
            "토지 / 대지",
            "서울 강남구 역삼동 123",
            "서울 강남구 테헤란로 1",
            500_000_000,
            700_000_000,
            "일반경쟁",
            "입찰진행중",
            "20260325100000",
            "20260327170000",
            3,
            42,
            "https://example.com/img.jpg");

        item.PlnmNo.Should().Be(100);
        item.PbctNo.Should().Be(200);
        item.CltrNo.Should().Be(300);
        item.CltrNm.Should().Be("서울 강남구 역삼동");
        item.CtgrFullNm.Should().Be("토지 / 대지");
        item.LdnmAdrs.Should().Be("서울 강남구 역삼동 123");
        item.NmrdAdrs.Should().Be("서울 강남구 테헤란로 1");
        item.MinBidPrc.Should().Be(500_000_000);
        item.ApslAsesAvgAmt.Should().Be(700_000_000);
        item.BidMtdNm.Should().Be("일반경쟁");
        item.PbctCltrStatNm.Should().Be("입찰진행중");
        item.UscbdCnt.Should().Be(3);
        item.IqryCnt.Should().Be(42);
        item.CltrImgFiles.Should().Be("https://example.com/img.jpg");
        item.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        item.UpdatedAt.Should().Be(item.CreatedAt);
    }

    [Fact]
    public void Create_ShouldParseDateTimesToKst()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20260325100000",
            pbctClsDtm: "20260327170000");

        item.PbctBegnDtm.Offset.Should().Be(TimeSpan.Zero);
        item.PbctBegnDtm.Year.Should().Be(2026);
        item.PbctBegnDtm.Month.Should().Be(3);
        item.PbctBegnDtm.Day.Should().Be(25);
        item.PbctBegnDtm.Hour.Should().Be(1);

        item.PbctClsDtm.Offset.Should().Be(TimeSpan.Zero);
        item.PbctClsDtm.Day.Should().Be(27);
        item.PbctClsDtm.Hour.Should().Be(8);
    }

    [Fact]
    public void Create_WithNullImage_ShouldSetNull()
    {
        KamcoAuctionItem item = CreateDefaultItem();

        item.CltrImgFiles.Should().BeNull();
    }

    [Fact]
    public void UpdateFromSnapshot_ShouldUpdateMutableFields()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            cltrNm: "old name",
            minBidPrc: 100,
            apslAsesAvgAmt: 200);

        DateTimeOffset originalCreatedAt = item.CreatedAt;

        item.UpdateFromSnapshot(
            "new name",
            "new category",
            "new addr1",
            "new addr2",
            999,
            1500,
            "new method",
            "new status",
            "20260301100000",
            "20260305170000",
            5,
            100,
            "https://new-image.jpg");

        item.PlnmNo.Should().Be(1);
        item.PbctNo.Should().Be(2);
        item.CltrNo.Should().Be(3);
        item.CltrNm.Should().Be("new name");
        item.CtgrFullNm.Should().Be("new category");
        item.LdnmAdrs.Should().Be("new addr1");
        item.NmrdAdrs.Should().Be("new addr2");
        item.MinBidPrc.Should().Be(999);
        item.ApslAsesAvgAmt.Should().Be(1500);
        item.BidMtdNm.Should().Be("new method");
        item.PbctCltrStatNm.Should().Be("new status");
        item.PbctBegnDtm.Day.Should().Be(1);
        item.PbctBegnDtm.Month.Should().Be(3);
        item.PbctClsDtm.Day.Should().Be(5);
        item.UscbdCnt.Should().Be(5);
        item.IqryCnt.Should().Be(100);
        item.CltrImgFiles.Should().Be("https://new-image.jpg");
        item.CreatedAt.Should().Be(originalCreatedAt);
        item.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public void IsActive_WhenInBiddingWindow_ShouldReturnTrue()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20200101000000",
            pbctClsDtm: "20991231235959");

        item.IsActive(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenBeforeStart_ShouldReturnFalse()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20991201000000",
            pbctClsDtm: "20991231235959");

        item.IsActive(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenAfterClose_ShouldReturnFalse()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20200101000000",
            pbctClsDtm: "20200102000000");

        item.IsActive(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsClosed_WhenPastCloseDate_ShouldReturnTrue()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20200101000000",
            pbctClsDtm: "20200102000000");

        item.IsClosed(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsClosed_WhenBeforeCloseDate_ShouldReturnFalse()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            pbctBegnDtm: "20200101000000",
            pbctClsDtm: "20991231235959");

        item.IsClosed(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void DiscountRate_ShouldCalculateCorrectly()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            minBidPrc: 700_000_000,
            apslAsesAvgAmt: 1_000_000_000);

        item.DiscountRate().Should().Be(30.0);
    }

    [Fact]
    public void DiscountRate_WhenAppraisalIsZero_ShouldReturnZero()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            minBidPrc: 100,
            apslAsesAvgAmt: 0);

        item.DiscountRate().Should().Be(0);
    }

    [Fact]
    public void DiscountRate_WhenMinBidEqualsAppraisal_ShouldReturnZero()
    {
        KamcoAuctionItem item = CreateDefaultItem(
            minBidPrc: 1_000_000,
            apslAsesAvgAmt: 1_000_000);

        item.DiscountRate().Should().Be(0);
    }

    [Fact]
    public void ParseKamcoDateTime_ShouldParseValidFormat()
    {
        DateTimeOffset result = KamcoAuctionItem.ParseKamcoDateTime("20260315143000");

        result.Year.Should().Be(2026);
        result.Month.Should().Be(3);
        result.Day.Should().Be(15);
        result.Hour.Should().Be(5);
        result.Minute.Should().Be(30);
        result.Second.Should().Be(0);
        result.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ParseKamcoDateTime_WithInvalidFormat_ShouldThrow()
    {
        Action act = () => KamcoAuctionItem.ParseKamcoDateTime("invalid");

        act.Should().Throw<FormatException>();
    }

    private static KamcoAuctionItem CreateDefaultItem(
        long plnmNo = 1,
        long pbctNo = 2,
        long cltrNo = 3,
        string cltrNm = "n",
        string ctgrFullNm = "c",
        string ldnmAdrs = "a1",
        string nmrdAdrs = "a2",
        long minBidPrc = 100,
        long apslAsesAvgAmt = 200,
        string bidMtdNm = "b",
        string pbctCltrStatNm = "s",
        string pbctBegnDtm = "20260101090000",
        string pbctClsDtm = "20260102090000",
        int uscbdCnt = 0,
        int iqryCnt = 0,
        string? cltrImgFiles = null)
    {
        return KamcoAuctionItem.Create(
            plnmNo,
            pbctNo,
            cltrNo,
            cltrNm,
            ctgrFullNm,
            ldnmAdrs,
            nmrdAdrs,
            minBidPrc,
            apslAsesAvgAmt,
            bidMtdNm,
            pbctCltrStatNm,
            pbctBegnDtm,
            pbctClsDtm,
            uscbdCnt,
            iqryCnt,
            cltrImgFiles);
    }
}
