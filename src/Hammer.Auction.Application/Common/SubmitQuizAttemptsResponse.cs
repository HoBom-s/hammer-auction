namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 일괄 풀이 제출 결과 DTO.
/// </summary>
public sealed record SubmitQuizAttemptsResponse(
    int Total,
    int Correct,
    IReadOnlyList<QuizAttemptResponse> Attempts);
