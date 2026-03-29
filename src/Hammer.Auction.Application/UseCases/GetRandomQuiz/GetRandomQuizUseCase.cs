using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;

namespace Hammer.Auction.Application.UseCases.GetRandomQuiz;

/// <summary>
/// Retrieves random quiz questions from the repository.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class GetRandomQuizUseCase(IQuizRepository repository) : IGetRandomQuizUseCase
{
    private const int MaxCount = 10;

    /// <inheritdoc />
    public async Task<IReadOnlyList<QuizResponse>> ExecuteAsync(int count, CancellationToken ct = default)
    {
        if (count is < 1 or > MaxCount)
            throw new BadRequestException($"Count must be between 1 and {MaxCount}, but was {count}.");

        IReadOnlyList<Quiz> quizzes = await repository.GetRandomAsync(count, ct);

        if (quizzes.Count == 0)
            throw new NotFoundException("No quiz questions available.");

        return quizzes.Select(QuizResponse.FromEntity).ToList();
    }
}
