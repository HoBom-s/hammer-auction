using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Exceptions;

/// <summary>
///     Represents a 400 Bad Request error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class BadRequestException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
    /// </summary>
    public BadRequestException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
    /// </summary>
    public BadRequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadRequestException" /> class.
    /// </summary>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
