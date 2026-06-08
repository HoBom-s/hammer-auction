namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 응답 DTO.
/// </summary>
public sealed record QuizResponse(
    long Id,
    string Question,
    IReadOnlyList<string> Choices,
    int CorrectIndex,
    string Explanation);
