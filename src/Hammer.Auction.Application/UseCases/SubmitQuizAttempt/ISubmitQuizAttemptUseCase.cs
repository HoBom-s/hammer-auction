using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.SubmitQuizAttempt;

/// <summary>
/// Use case contract for submitting a quiz attempt.
/// </summary>
public interface ISubmitQuizAttemptUseCase
{
    /// <summary>
    /// Submits a quiz attempt and returns the result.
    /// </summary>
    public Task<QuizAttemptResponse> ExecuteAsync(
        UserId userId,
        QuizId quizId,
        SubmitQuizAttemptRequest request,
        CancellationToken ct = default);
}
