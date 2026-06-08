using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.SubmitQuizAttempts;

/// <summary>
/// Use case contract for submitting all quiz attempts of a set in a single batch.
/// </summary>
public interface ISubmitQuizAttemptsUseCase
{
    /// <summary>
    /// Submits a batch of quiz attempts and sends a single completion notification.
    /// </summary>
    public Task<SubmitQuizAttemptsResponse> ExecuteAsync(
        UserId userId,
        SubmitQuizAttemptsRequest request,
        CancellationToken ct = default);
}
