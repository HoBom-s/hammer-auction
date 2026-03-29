namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 수정 요청 DTO.
/// </summary>
public sealed record UpdateQuizRequest(
    string Question,
    string Choice1,
    string Choice2,
    string Choice3,
    string Choice4,
    int CorrectIndex,
    string Explanation);
