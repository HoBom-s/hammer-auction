using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.Ports;

/// <summary>
/// HTTP client port for quiz operations via hammer-internal.
/// </summary>
public interface IQuizClient
{
    /// <summary>
    /// Retrieves random quiz questions from hammer-internal.
    /// </summary>
    public Task<IReadOnlyList<QuizResponse>> GetRandomAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a quiz by its identifier from hammer-internal.
    /// </summary>
    public Task<QuizResponse?> GetByIdAsync(long id, CancellationToken ct = default);
}
