using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Auction.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for quiz questions.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class QuizRepository(AuctionDbContext db) : IQuizRepository
{
    /// <inheritdoc />
    public async Task<Quiz?> GetByIdAsync(QuizId id, CancellationToken ct = default) =>
        await db.Quizzes.FirstOrDefaultAsync(q => q.Id == id.Value, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Quiz>> GetRandomAsync(int count, CancellationToken ct = default)
    {
        return await db.Quizzes
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);
}
