using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="KamcoAuctionItemResponse"/>.
/// </summary>
public sealed class KamcoAuctionItemResponseTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFields()
    {
        var entity = KamcoAuctionItem.Create(
            100,
            200,
            300,
            "Test Item",
            "토지 > 대지",
            "서울시 강남구",
            "테헤란로 123",
            80_000_000,
            100_000_000,
            "전자입찰",
            "공매진행",
            "20260315100000",
            "20260320170000",
            2,
            150,
            "https://img.example.com/1.jpg");

        var response = KamcoAuctionItemResponse.FromEntity(entity);

        response.PlnmNo.Should().Be(100);
        response.PbctNo.Should().Be(200);
        response.CltrNo.Should().Be(300);
        response.CltrNm.Should().Be("Test Item");
        response.CtgrFullNm.Should().Be("토지 > 대지");
        response.LdnmAdrs.Should().Be("서울시 강남구");
        response.NmrdAdrs.Should().Be("테헤란로 123");
        response.MinBidPrc.Should().Be(80_000_000);
        response.ApslAsesAvgAmt.Should().Be(100_000_000);
        response.BidMtdNm.Should().Be("전자입찰");
        response.PbctCltrStatNm.Should().Be("공매진행");
        response.UscbdCnt.Should().Be(2);
        response.IqryCnt.Should().Be(150);
        response.CltrImgFiles.Should().Be("https://img.example.com/1.jpg");
        response.DiscountRate.Should().Be(20.0);
        response.PbctBegnDtm.Day.Should().Be(15);
        response.PbctClsDtm.Day.Should().Be(20);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FromEntity_WithNullEntity_ShouldThrow()
    {
        Action act = () => KamcoAuctionItemResponse.FromEntity(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromEntity_WithZeroAppraisal_ShouldReturnZeroDiscount()
    {
        var entity = KamcoAuctionItem.Create(
            1,
            1,
            1,
            "Item",
            "Cat",
            "Addr",
            "Road",
            50,
            0,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);

        var response = KamcoAuctionItemResponse.FromEntity(entity);

        response.DiscountRate.Should().Be(0);
    }

    [Fact]
    public void FromEntity_WithNullImages_ShouldMapNullImages()
    {
        var entity = KamcoAuctionItem.Create(
            1,
            1,
            1,
            "Item",
            "Cat",
            "Addr",
            "Road",
            100,
            200,
            "Method",
            "Status",
            "20260101090000",
            "20260102090000",
            0,
            0,
            null);

        var response = KamcoAuctionItemResponse.FromEntity(entity);

        response.CltrImgFiles.Should().BeNull();
    }
}
