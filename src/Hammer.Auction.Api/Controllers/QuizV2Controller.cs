using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Api.ModelBinding;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempts;
using Hammer.Auction.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Hammer.Auction.Api.Controllers;

/// <summary>
///     공매 지식 퀴즈 API v2.
///     한 세트의 퀴즈 풀이를 일괄 제출하고, 세트를 모두 제출하면 완료 알림을 1건 발송합니다.
/// </summary>
[ApiController]
[Route("v2/quizzes")]
[Tags("Quiz")]
[SuppressMessage("Microsoft.Design", "CA1515", Justification = "MVC requires public controllers")]
public sealed class QuizV2Controller(ISubmitQuizAttemptsUseCase submitQuizAttempts) : ControllerBase
{
    /// <summary>
    ///     한 세트의 퀴즈 풀이를 일괄 제출합니다.
    /// </summary>
    /// <param name="request">일괄 풀이 제출 요청.</param>
    /// <param name="userId">Gateway가 X-User-Id 헤더로 전달한 사용자 식별자.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("attempts")]
    [ProducesResponseType<SubmitQuizAttemptsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitQuizAttemptsResponse>> SubmitAttemptsAsync(
        [FromBody] SubmitQuizAttemptsRequest request,
        [CurrentUserId] UserId userId,
        CancellationToken ct = default)
    {
        SubmitQuizAttemptsResponse result = await submitQuizAttempts.ExecuteAsync(userId, request, ct);

        return CreatedAtAction(null, result);
    }
}
