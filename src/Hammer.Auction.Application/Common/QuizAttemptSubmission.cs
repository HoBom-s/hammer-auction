namespace Hammer.Auction.Application.Common;

/// <summary>
/// 일괄 제출에 포함되는 개별 퀴즈 풀이 항목.
/// </summary>
public sealed record QuizAttemptSubmission(long QuizId, int SelectedIndex);
