using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Exceptions;

/// <summary>
/// Represents a 403 Forbidden error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
