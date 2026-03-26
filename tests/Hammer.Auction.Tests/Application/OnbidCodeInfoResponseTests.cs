using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="OnbidCodeInfoResponse"/>.
/// </summary>
public sealed class OnbidCodeInfoResponseTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFields()
    {
        var entity = OnbidCodeInfo.Create(
            "CTG01",
            "토지",
            "ROOT",
            "전체");

        var response = OnbidCodeInfoResponse.FromEntity(entity);

        response.CtgrId.Should().Be("CTG01");
        response.CtgrNm.Should().Be("토지");
        response.CtgrHirkId.Should().Be("ROOT");
        response.CtgrHirkNm.Should().Be("전체");
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FromEntity_WithNullEntity_ShouldThrow()
    {
        Action act = () => OnbidCodeInfoResponse.FromEntity(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
