using System.Diagnostics.CodeAnalysis;

namespace Hammer.Auction.Application.Exceptions;

/// <summary>
/// Represents a 503 Service Unavailable error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ServiceUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
    /// </summary>
    public ServiceUnavailableException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
    /// </summary>
    public ServiceUnavailableException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
    /// </summary>
    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
