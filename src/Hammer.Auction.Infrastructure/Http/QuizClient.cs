using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Hammer.Auction.Infrastructure.Http;

/// <summary>
///     hammer-internal API를 호출하여 퀴즈를 조회한다.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class QuizClient(HttpClient httpClient, ILogger<QuizClient> logger) : IQuizClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IReadOnlyList<QuizResponse>> GetRandomAsync(int count, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"internal/quizzes/random?count={count}", UriKind.Relative),
                ct);

            response.EnsureSuccessStatusCode();

            List<QuizResponse>? result = await response.Content.ReadFromJsonAsync<List<QuizResponse>>(JsonOptions, ct);
            return result ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch random quizzes from hammer-internal");
            return [];
        }
    }

    public async Task<QuizResponse?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                new Uri($"internal/quizzes/{id}", UriKind.Relative),
                ct);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<QuizResponse>(JsonOptions, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to fetch quiz {QuizId} from hammer-internal", id);
            return null;
        }
    }
}
