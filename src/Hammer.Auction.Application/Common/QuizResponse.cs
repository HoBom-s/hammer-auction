using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 응답 DTO.
/// </summary>
public sealed record QuizResponse(
    long Id,
    string Question,
    IReadOnlyList<string> Choices,
    int CorrectIndex,
    string Explanation)
{
    /// <summary>
    /// Converts a <see cref="Quiz"/> entity to a response DTO.
    /// </summary>
    public static QuizResponse FromEntity(Quiz entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new QuizResponse(
            entity.Id,
            entity.Question,
            [entity.Choice1, entity.Choice2, entity.Choice3, entity.Choice4],
            entity.CorrectIndex,
            entity.Explanation);
    }
}
