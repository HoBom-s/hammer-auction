using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Api.ModelBinding;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetRandomQuiz;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempt;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     공매 지식 퀴즈 API.
///     랜덤 퀴즈 문제 제공 및 풀이 제출을 처리합니다.
/// </summary>
[ApiController]
[Route("quizzes")]
[Tags("Quiz")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource controller")]
public sealed class QuizController(
    IGetRandomQuizUseCase getRandomQuiz,
    ISubmitQuizAttemptUseCase submitQuizAttempt) : ControllerBase
{
    /// <summary>
    ///     랜덤 퀴즈를 조회합니다 (기본 3문제).
    /// </summary>
    /// <param name="count">문제 수 (기본값: 3, 최대: 10).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("random")]
    [ProducesResponseType<IReadOnlyList<QuizResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuizResponse>>> GetRandomAsync(
        [FromQuery] int count = 3,
        CancellationToken ct = default)
    {
        IReadOnlyList<QuizResponse> result = await getRandomQuiz.ExecuteAsync(count, ct);

        return Ok(result);
    }

    /// <summary>
    ///     퀴즈 풀이를 제출합니다.
    /// </summary>
    /// <param name="id">퀴즈 식별자.</param>
    /// <param name="request">풀이 제출 요청.</param>
    /// <param name="userId">Gateway가 X-User-Id 헤더로 전달한 사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{id}/attempts")]
    [ProducesResponseType<QuizAttemptResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizAttemptResponse>> SubmitAttemptAsync(
        QuizId id,
        [FromBody] SubmitQuizAttemptRequest request,
        [CurrentUserId] UserId userId,
        CancellationToken ct = default)
    {
        QuizAttemptResponse result = await submitQuizAttempt.ExecuteAsync(userId, id, request, ct);

        return CreatedAtAction(null, result);
    }
}
