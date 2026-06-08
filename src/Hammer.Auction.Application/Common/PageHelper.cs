using Hammer.Auction.Application.Exceptions;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Helper for pagination.
/// </summary>
public static class PageHelper
{
    private const int MaxSize = 100;

    /// <summary>
    ///     Validates page and size parameters.
    /// </summary>
    public static void Validate(int page, int size)
    {
        if (page < 1)
            throw new BadRequestException($"Page must be at least 1, but was {page}.");

        if (size is < 1 or > MaxSize)
            throw new BadRequestException($"Size must be between 1 and {MaxSize}, but was {size}.");
    }

    /// <summary>
    ///     Get total pages count.
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int size) =>
        totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / size);
}
