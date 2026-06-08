namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 풀이 제출 요청 DTO.
/// </summary>
public sealed record SubmitQuizAttemptRequest(int SelectedIndex);
