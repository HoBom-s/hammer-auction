using FluentAssertions;
using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Tests.Domain;

/// <summary>
/// Tests for <see cref="SearchLog"/>.
/// </summary>
public sealed class SearchLogTests
{
    [Fact]
    public void Create_ShouldNormalizeKeywordToUppercase()
    {
        var log = SearchLog.Create("  Seoul Apartment  ", "user-1");

        log.Keyword.Should().Be("SEOUL APARTMENT");
    }

    [Fact]
    public void Create_ShouldSetUserId()
    {
        var log = SearchLog.Create("keyword", "user-42");

        log.UserId.Should().Be("user-42");
    }

    [Fact]
    public void Create_ShouldSetSearchedAtToUtcNow()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;

        var log = SearchLog.Create("keyword", "user-1");

        log.SearchedAt.Should().BeOnOrAfter(before);
        log.SearchedAt.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidKeyword_ShouldThrow(string? keyword)
    {
        Action act = () => SearchLog.Create(keyword!, "user-1");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUserId_ShouldThrow(string? userId)
    {
        Action act = () => SearchLog.Create("keyword", userId!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldTrimKeyword()
    {
        var log = SearchLog.Create("  hello  ", "user-1");

        log.Keyword.Should().Be("HELLO");
    }
}
