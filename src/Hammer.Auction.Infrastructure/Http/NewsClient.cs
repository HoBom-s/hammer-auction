using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Http;

/// <summary>
///     hammer-support API를 호출하여 뉴스를 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class NewsClient(HttpClient httpClient, ILogger<NewsClient> logger) : INewsClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <inheritdoc />
    public async Task<IReadOnlyList<NewsResponse>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"api/news/recent?count={count}", UriKind.Relative),
                ct);

            response.EnsureSuccessStatusCode();

            List<NewsResponse>? result = await response.Content.ReadFromJsonAsync<List<NewsResponse>>(_jsonOptions, ct);
            return result ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch recent news from hammer-support");
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<NewsResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"api/news/{id}", UriKind.Relative),
                ct);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<NewsResponse>(_jsonOptions, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch news {NewsId} from hammer-support", id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<PagedResponse<NewsResponse>> GetPagedAsync(int page, int size, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"api/news?page={page}&size={size}", UriKind.Relative),
                ct);

            response.EnsureSuccessStatusCode();

            PagedResponse<NewsResponse>? result = await response.Content.ReadFromJsonAsync<PagedResponse<NewsResponse>>(_jsonOptions, ct);
            return result ?? EmptyPage(page, size);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch paged news from hammer-support");
            return EmptyPage(page, size);
        }
    }

    /// <inheritdoc />
    public async Task<PagedResponse<NewsResponse>> SearchByTitleAsync(string keyword, int page, int size, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"api/news/search?keyword={Uri.EscapeDataString(keyword)}&page={page}&size={size}", UriKind.Relative),
                ct);

            response.EnsureSuccessStatusCode();

            PagedResponse<NewsResponse>? result = await response.Content.ReadFromJsonAsync<PagedResponse<NewsResponse>>(_jsonOptions, ct);
            return result ?? EmptyPage(page, size);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to search news from hammer-support");
            return EmptyPage(page, size);
        }
    }

    private static PagedResponse<NewsResponse> EmptyPage(int page, int size) =>
        new([], page, size, 0, 0);
}
