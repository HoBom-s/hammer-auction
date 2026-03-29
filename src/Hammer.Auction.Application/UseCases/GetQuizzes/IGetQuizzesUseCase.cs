using Hammer.Auction.Application.Common;

namespace Hammer.Auction.Application.UseCases.GetQuizzes;

/// <summary>
/// Use case contract for retrieving a paginated list of quizzes.
/// </summary>
public interface IGetQuizzesUseCase
{
    /// <summary>
    /// Retrieves a paginated list of quizzes.
    /// </summary>
    public Task<PagedResponse<QuizResponse>> ExecuteAsync(int page, int size, CancellationToken ct = default);
}
