using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     퀴즈 풀이 기록.
/// </summary>
public sealed class QuizAttempt
{
    private QuizAttempt()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the user identifier.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the quiz identifier (nullable — quiz may be deleted).
    /// </summary>
    public long? QuizId { get; private set; }

    /// <summary>
    ///     Gets the zero-based index of the selected answer.
    /// </summary>
    public int SelectedIndex { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the selected answer was correct.
    /// </summary>
    public bool IsCorrect { get; private set; }

    /// <summary>
    ///     Gets the timestamp when the attempt was made.
    /// </summary>
    public DateTimeOffset AttemptedAt { get; private set; }

    /// <summary>
    ///     Creates a new quiz attempt record.
    /// </summary>
    public static QuizAttempt Create(UserId userId, QuizId quizId, int selectedIndex, bool isCorrect)
    {
        return new QuizAttempt
        {
            UserId = userId.Value,
            QuizId = quizId.Value,
            SelectedIndex = selectedIndex,
            IsCorrect = isCorrect,
            AttemptedAt = DateTimeOffset.UtcNow,
        };
    }
}
