namespace Hammer.Auction.Application;

/// <summary>
/// hammer-user API 연결 설정.
/// </summary>
public sealed class UserApiSettings
{
    /// <summary>
    /// Gets or sets the base URI of the hammer-user API.
    /// </summary>
    public Uri? BaseUri { get; set; }
}
