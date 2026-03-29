using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.DeleteQuiz;

/// <summary>
/// Use case contract for deleting a quiz question.
/// </summary>
public interface IDeleteQuizUseCase
{
    /// <summary>
    /// Deletes a quiz question by its identifier.
    /// </summary>
    public Task ExecuteAsync(QuizId id, CancellationToken ct = default);
}
