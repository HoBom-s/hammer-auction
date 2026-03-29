using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.CreateQuiz;
using Hammer.Auction.Application.UseCases.DeleteQuiz;
using Hammer.Auction.Application.UseCases.GetQuizzes;
using Hammer.Auction.Application.UseCases.UpdateQuiz;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     퀴즈 관리 API (내부용).
///     퀴즈 CRUD 작업을 제공합니다.
/// </summary>
[ApiController]
[Route("internal/quizzes")]
[Tags("Quiz Admin")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
[SuppressMessage("SonarAnalyzer.CSharp", "S6960", Justification = "Single resource CRUD controller")]
public sealed class InternalQuizController(
    IGetQuizzesUseCase getQuizzes,
    ICreateQuizUseCase createQuiz,
    IUpdateQuizUseCase updateQuiz,
    IDeleteQuizUseCase deleteQuiz) : ControllerBase
{
    /// <summary>
    ///     퀴즈 목록을 페이징 조회합니다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작, 기본값: 1).</param>
    /// <param name="size">페이지당 항목 수 (기본값: 20, 최대: 100).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<QuizResponse>>> GetQuizzesAsync(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        PagedResponse<QuizResponse> result = await getQuizzes.ExecuteAsync(page, size, ct);

        return Ok(result);
    }

    /// <summary>
    ///     새 퀴즈를 생성합니다.
    /// </summary>
    /// <param name="request">퀴즈 생성 요청.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType<QuizResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<QuizResponse>> CreateQuizAsync(
        [FromBody] CreateQuizRequest request,
        CancellationToken ct = default)
    {
        QuizResponse result = await createQuiz.ExecuteAsync(request, ct);

        return CreatedAtAction(null, result);
    }

    /// <summary>
    ///     퀴즈를 수정합니다.
    /// </summary>
    /// <param name="id">퀴즈 식별자.</param>
    /// <param name="request">퀴즈 수정 요청.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("{id}")]
    [ProducesResponseType<QuizResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizResponse>> UpdateQuizAsync(
        QuizId id,
        [FromBody] UpdateQuizRequest request,
        CancellationToken ct = default)
    {
        QuizResponse result = await updateQuiz.ExecuteAsync(id, request, ct);

        return Ok(result);
    }

    /// <summary>
    ///     퀴즈를 삭제합니다.
    /// </summary>
    /// <param name="id">퀴즈 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuizAsync(QuizId id, CancellationToken ct = default)
    {
        await deleteQuiz.ExecuteAsync(id, ct);

        return NoContent();
    }
}
