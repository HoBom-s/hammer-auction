using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Ports;

/// <summary>
/// Repository port for quiz questions.
/// </summary>
public interface IQuizRepository
{
    /// <summary>
    /// Retrieves a quiz by its identifier.
    /// </summary>
    public Task<Quiz?> GetByIdAsync(QuizId id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves random quiz questions.
    /// </summary>
    public Task<IReadOnlyList<Quiz>> GetRandomAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default);
}
