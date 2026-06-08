namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 일괄 풀이 제출 요청 DTO.
/// </summary>
public sealed record SubmitQuizAttemptsRequest(IReadOnlyList<QuizAttemptSubmission> Attempts);
