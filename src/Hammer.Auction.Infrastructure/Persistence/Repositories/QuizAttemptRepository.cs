using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for quiz attempt records.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class QuizAttemptRepository(AuctionDbContext db) : IQuizAttemptRepository
{
    /// <inheritdoc />
    public void Add(QuizAttempt attempt) =>
        db.QuizAttempts.Add(attempt);

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<QuizAttempt> Items, int TotalCount)> GetByUserIdAsync(
        string userId,
        int page,
        int size,
        CancellationToken ct = default)
    {
        IQueryable<QuizAttempt> query = db.QuizAttempts.Where(a => a.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        List<QuizAttempt> items = await query
            .OrderByDescending(a => a.AttemptedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
