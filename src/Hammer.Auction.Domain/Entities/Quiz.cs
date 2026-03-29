namespace Hammer.Auction.Domain.Entities;

/// <summary>
///     공매 지식 퀴즈 문제.
/// </summary>
public sealed class Quiz
{
    private Quiz()
    {
    }

    /// <summary>
    ///     Gets the surrogate primary key.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///     Gets the quiz question (질문).
    /// </summary>
    public string Question { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the first choice (선택지 1).
    /// </summary>
    public string Choice1 { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the second choice (선택지 2).
    /// </summary>
    public string Choice2 { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the third choice (선택지 3).
    /// </summary>
    public string Choice3 { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the fourth choice (선택지 4).
    /// </summary>
    public string Choice4 { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the zero-based index of the correct answer.
    /// </summary>
    public int CorrectIndex { get; private set; }

    /// <summary>
    ///     Gets the explanation for the correct answer (해설).
    /// </summary>
    public string Explanation { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Creates a new quiz question.
    /// </summary>
    public static Quiz Create(
        string question,
        string choice1,
        string choice2,
        string choice3,
        string choice4,
        int correctIndex,
        string explanation)
    {
        return new Quiz
        {
            Question = question,
            Choice1 = choice1,
            Choice2 = choice2,
            Choice3 = choice3,
            Choice4 = choice4,
            CorrectIndex = correctIndex,
            Explanation = explanation,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    ///     Updates the quiz question fields.
    /// </summary>
    public void Update(
        string question,
        string choice1,
        string choice2,
        string choice3,
        string choice4,
        int correctIndex,
        string explanation)
    {
        Question = question;
        Choice1 = choice1;
        Choice2 = choice2;
        Choice3 = choice3;
        Choice4 = choice4;
        CorrectIndex = correctIndex;
        Explanation = explanation;
    }
}
