using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="PageHelper"/>.
/// </summary>
public sealed class PageHelperTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidPage_ShouldThrow(int page)
    {
        Action act = () => PageHelper.Validate(page, 20);

        act.Should().Throw<BadRequestException>().WithMessage("*Page*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(999)]
    public void Validate_WithInvalidSize_ShouldThrow(int size)
    {
        Action act = () => PageHelper.Validate(1, size);

        act.Should().Throw<BadRequestException>().WithMessage("*Size*");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(999, 50)]
    public void Validate_WithValidInput_ShouldNotThrow(int page, int size)
    {
        Action act = () => PageHelper.Validate(page, size);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(100, 10, 10)]
    public void CalculateTotalPages_ShouldReturnCorrectValue(int totalCount, int size, int expected)
    {
        var result = PageHelper.CalculateTotalPages(totalCount, size);

        result.Should().Be(expected);
    }
}
