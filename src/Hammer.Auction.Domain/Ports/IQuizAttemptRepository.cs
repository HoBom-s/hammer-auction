using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for quiz attempt records.
/// </summary>
public interface IQuizAttemptRepository
{
    /// <summary>
    /// Adds a new quiz attempt to the context.
    /// </summary>
    public void Add(QuizAttempt attempt);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves paginated quiz attempts for a specific user.
    /// </summary>
    public Task<(IReadOnlyList<QuizAttempt> Items, int TotalCount)> GetByUserIdAsync(
        string userId,
        int page,
        int size,
        CancellationToken ct = default);
}
