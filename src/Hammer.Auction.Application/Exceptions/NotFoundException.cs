using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Exceptions;

/// <summary>
/// Represents a 404 Not Found error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
