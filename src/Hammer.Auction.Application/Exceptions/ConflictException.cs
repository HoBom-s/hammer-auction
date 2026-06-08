using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Exceptions;

/// <summary>
/// Represents a 409 Conflict error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
