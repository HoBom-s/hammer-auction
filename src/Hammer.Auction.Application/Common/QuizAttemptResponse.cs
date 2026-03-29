using Hammer.Auction.Domain.Entities;

namespace Hammer.Auction.Application.Common;

/// <summary>
/// 퀴즈 풀이 기록 응답 DTO.
/// </summary>
public sealed record QuizAttemptResponse(
    long Id,
    long? QuizId,
    int SelectedIndex,
    bool IsCorrect,
    DateTimeOffset AttemptedAt)
{
    /// <summary>
    /// Converts a <see cref="QuizAttempt"/> entity to a response DTO.
    /// </summary>
    public static QuizAttemptResponse FromEntity(QuizAttempt entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new QuizAttemptResponse(
            entity.Id,
            entity.QuizId,
            entity.SelectedIndex,
            entity.IsCorrect,
            entity.AttemptedAt);
    }
}
