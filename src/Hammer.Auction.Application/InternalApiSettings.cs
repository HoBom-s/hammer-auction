namespace Hammer.Auction.Application;

/// <summary>
/// hammer-internal API 연결 설정.
/// </summary>
public sealed class InternalApiSettings
{
    /// <summary>
    /// Gets or sets the base URI of the hammer-internal API.
    /// </summary>
    public Uri? BaseUri { get; set; }
}
