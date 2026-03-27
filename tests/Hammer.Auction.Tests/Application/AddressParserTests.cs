using FluentAssertions;
using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="AddressParser"/>.
/// </summary>
public sealed class AddressParserTests
{
    [Theory]
    [InlineData("부산광역시 금정구 부곡동 970 롯데캐슬디아망 제103동 제9층 제901호", "부곡동", "970")]
    [InlineData("경기도 남양주시 진접읍 내각리 165-77", "내각리", "165-77")]
    [InlineData("서울특별시 강북구 번동 234 번동주공아파트 제405동 제1층 제102호", "번동", "234")]
    [InlineData("경기도 파주시 상지석동 554-277 라엘하우스 제102동 제1층 제102호", "상지석동", "554-277")]
    [InlineData("경상북도 포항시 남구 호미곶면 강사리 431-2", "강사리", "431-2")]
    [InlineData("경기도 수원시 팔달구 매산로3가 28 수원세무서", "매산로3가", "28")]
    [InlineData("경상남도 창원시 의창구 용동 산 12-1", "용동", "산12-1")]
    [InlineData("경기도 가평군 청평면 청평리 산6-104", "청평리", "산6-104")]
    [InlineData("경기도 가평군 청평면 청평리 산 6-104", "청평리", "산6-104")]
    [InlineData("서울특별시 종로구 종로1가 1-5 종로빌딩", "종로1가", "1-5")]
    [InlineData("부산광역시 금정구 부곡동 970 101동 제9층", "부곡동", "970")]
    [InlineData("부산광역시  금정구  부곡동  970  롯데캐슬", "부곡동", "970")]
    public void ParseLocation_ShouldExtractUmdNmAndJibun(
        string address,
        string expectedUmd,
        string expectedJibun)
    {
        (string? UmdNm, string? Jibun) result = AddressParser.ParseLocation(address);

        result.UmdNm.Should().Be(expectedUmd);
        result.Jibun.Should().Be(expectedJibun);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("보관중인 건설공제조합 출자증권")]
    [InlineData("서울특별시 강남구 테헤란로 152 강남파이낸스센터")]
    [InlineData("경기도 성남시 분당구 판교역로 235")]
    public void ParseLocation_WithUnparsableInput_ShouldReturnNulls(string? address)
    {
        (string? UmdNm, string? Jibun) result = AddressParser.ParseLocation(address);

        result.UmdNm.Should().BeNull();
        result.Jibun.Should().BeNull();
    }
}
