using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using Hammer.Auction.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Http;

/// <summary>
///     hammer-user API를 호출하여 디바이스 토큰을 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class DeviceTokenClient(HttpClient httpClient, ILogger<DeviceTokenClient> logger) : IDeviceTokenClient
{
    public async Task<string?> GetPushTokenAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"hammer-users/internal/users/{userId}/device-token", UriKind.Relative),
                ct);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            DeviceTokenDto? dto = await response.Content.ReadFromJsonAsync<DeviceTokenDto>(ct);
            return dto?.PushToken;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch device token for user {UserId}", userId);
            return null;
        }
    }

    private sealed record DeviceTokenDto(string PushToken);
}
