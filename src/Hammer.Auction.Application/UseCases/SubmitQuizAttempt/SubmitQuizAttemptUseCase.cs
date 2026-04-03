using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.SubmitQuizAttempt;

/// <summary>
/// Submits a quiz attempt: checks correctness and persists the record.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class SubmitQuizAttemptUseCase(
    IQuizRepository quizRepository,
    IQuizAttemptRepository attemptRepository,
    IDeviceTokenClient deviceTokenClient,
    INotificationSender notificationSender) : ISubmitQuizAttemptUseCase
{
    /// <inheritdoc />
    public async Task<QuizAttemptResponse> ExecuteAsync(
        UserId userId,
        QuizId quizId,
        SubmitQuizAttemptRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quiz quiz = await quizRepository.GetByIdAsync(quizId, ct)
            ?? throw new NotFoundException($"Quiz {quizId} not found.");

        var isCorrect = quiz.CorrectIndex == request.SelectedIndex;

        var attempt = QuizAttempt.Create(userId, quizId, request.SelectedIndex, isCorrect);

        attemptRepository.Add(attempt);

        var pushToken = await deviceTokenClient.GetPushTokenAsync(userId.Value, ct);
        if (pushToken is not null)
        {
            notificationSender.Send(new NotificationPayload(
                "quiz_result",
                pushToken,
                new Dictionary<string, string>
                {
                    ["isCorrect"] = isCorrect.ToString(),
                    ["question"] = quiz.Question,
                }));
        }

        await attemptRepository.SaveChangesAsync(ct);

        return QuizAttemptResponse.FromEntity(attempt);
    }
}
