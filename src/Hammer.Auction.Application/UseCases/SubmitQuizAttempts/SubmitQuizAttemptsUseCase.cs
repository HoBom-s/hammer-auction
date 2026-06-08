using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;

namespace Hammer.Auction.Application.UseCases.SubmitQuizAttempts;

/// <summary>
/// Submits all quiz attempts of a set as a batch, then notifies the user once — only after
/// the whole set has been submitted.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated via DI")]
internal sealed class SubmitQuizAttemptsUseCase(
    IQuizClient quizClient,
    IQuizAttemptRepository attemptRepository,
    IDeviceTokenClient deviceTokenClient,
    INotificationSender notificationSender,
    INotificationRepository notificationRepository,
    INotificationSettingRepository notificationSettingRepository) : ISubmitQuizAttemptsUseCase
{
    private const int MaxCount = 10;

    /// <inheritdoc />
    public async Task<SubmitQuizAttemptsResponse> ExecuteAsync(
        UserId userId,
        SubmitQuizAttemptsRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Attempts.Count is < 1 or > MaxCount)
        {
            throw new BadRequestException(
                $"Attempts count must be between 1 and {MaxCount}, but was {request.Attempts.Count}.");
        }

        var responses = new List<QuizAttemptResponse>(request.Attempts.Count);
        var correctCount = 0;

        foreach (QuizAttemptSubmission item in request.Attempts)
        {
            QuizResponse quiz = await quizClient.GetByIdAsync(item.QuizId, ct)
                ?? throw new NotFoundException($"Quiz {item.QuizId} not found.");

            var isCorrect = quiz.CorrectIndex == item.SelectedIndex;
            if (isCorrect)
                correctCount++;

            var attempt = QuizAttempt.Create(userId, new QuizId(item.QuizId), item.SelectedIndex, isCorrect);
            attemptRepository.Add(attempt);
            responses.Add(QuizAttemptResponse.FromEntity(attempt));
        }

        var total = request.Attempts.Count;

        await NotifyCompletionAsync(userId, total, correctCount, ct);

        await attemptRepository.SaveChangesAsync(ct);

        return new SubmitQuizAttemptsResponse(total, correctCount, responses);
    }

    private async Task NotifyCompletionAsync(UserId userId, int total, int correctCount, CancellationToken ct)
    {
        NotificationSetting? setting = await notificationSettingRepository.GetByUserIdAsync(userId.Value, ct);
        var notificationsEnabled = setting?.IsEnabled ?? true;

        if (!notificationsEnabled)
            return;

        var title = "오늘의 퀴즈 완료!";
        var body = $"총 {total}문제 중 {correctCount}문제를 맞혔어요.";

        var notification = Notification.Create(userId, NotificationTemplateKeys.QuizCompleted, title, body);
        notificationRepository.Add(notification);

        var pushToken = await deviceTokenClient.GetPushTokenAsync(userId.Value, ct);
        if (pushToken is null)
            return;

        notificationSender.Send(new NotificationPayload(
            NotificationTemplateKeys.QuizCompleted,
            pushToken,
            new Dictionary<string, string>
            {
                ["total"] = total.ToString(CultureInfo.InvariantCulture),
                ["correct"] = correctCount.ToString(CultureInfo.InvariantCulture),
            }));
    }
}
