using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetRandomQuiz;

/// <summary>
/// Use case contract for retrieving random quiz questions.
/// </summary>
public interface IGetRandomQuizUseCase
{
    /// <summary>
    /// Retrieves random quiz questions.
    /// </summary>
    public Task<IReadOnlyList<QuizResponse>> ExecuteAsync(int count, CancellationToken ct = default);
}
