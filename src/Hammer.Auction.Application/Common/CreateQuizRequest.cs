namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 생성 요청 DTO.
/// </summary>
public sealed record CreateQuizRequest(
    string Question,
    string Choice1,
    string Choice2,
    string Choice3,
    string Choice4,
    int CorrectIndex,
    string Explanation);
