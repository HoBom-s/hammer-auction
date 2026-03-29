using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.CreateQuiz;

/// <summary>
/// Use case contract for creating a quiz question.
/// </summary>
public interface ICreateQuizUseCase
{
    /// <summary>
    /// Creates a new quiz question.
    /// </summary>
    public Task<QuizResponse> ExecuteAsync(CreateQuizRequest request, CancellationToken ct = default);
}
