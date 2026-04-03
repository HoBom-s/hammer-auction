namespace Hammer.Auction.Application.Ports;

/// <summary>
/// hammer-user 서비스에서 사용자의 디바이스 푸시 토큰을 조회한다.
/// </summary>
public interface IDeviceTokenClient
{
    /// <summary>
    /// Returns the push token for the given user, or <c>null</c> on 404 / error.
    /// </summary>
    public Task<string?> GetPushTokenAsync(string userId, CancellationToken ct = default);
}
