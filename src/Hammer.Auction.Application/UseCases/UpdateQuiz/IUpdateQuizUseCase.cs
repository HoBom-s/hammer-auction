using Hammer.Auction.Application.Common;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.UpdateQuiz;

/// <summary>
/// Use case contract for updating a quiz question.
/// </summary>
public interface IUpdateQuizUseCase
{
    /// <summary>
    /// Updates an existing quiz question.
    /// </summary>
    public Task<QuizResponse> ExecuteAsync(QuizId id, UpdateQuizRequest request, CancellationToken ct = default);
}
