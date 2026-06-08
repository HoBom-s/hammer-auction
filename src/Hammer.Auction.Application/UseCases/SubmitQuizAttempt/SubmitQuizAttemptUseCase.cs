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
    IQuizClient quizClient,
    IQuizAttemptRepository attemptRepository,
    IDeviceTokenClient deviceTokenClient,
    INotificationSender notificationSender,
    INotificationRepository notificationRepository,
    INotificationSettingRepository notificationSettingRepository) : ISubmitQuizAttemptUseCase
{
    /// <inheritdoc />
    public async Task<QuizAttemptResponse> ExecuteAsync(
        UserId userId,
        QuizId quizId,
        SubmitQuizAttemptRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        QuizResponse quiz = await quizClient.GetByIdAsync(quizId.Value, ct)
            ?? throw new NotFoundException($"Quiz {quizId} not found.");

        var isCorrect = quiz.CorrectIndex == request.SelectedIndex;

        var attempt = QuizAttempt.Create(userId, quizId, request.SelectedIndex, isCorrect);

        attemptRepository.Add(attempt);

        NotificationSetting? setting = await notificationSettingRepository.GetByUserIdAsync(userId.Value, ct);
        var notificationsEnabled = setting?.IsEnabled ?? true;

        if (notificationsEnabled)
        {
            var title = isCorrect ? "정답입니다!" : "오답입니다";
            var body = isCorrect
                ? $"'{quiz.Question}' 퀴즈를 맞혔습니다."
                : $"'{quiz.Question}' 퀴즈의 정답을 확인해보세요.";

            var notification = Notification.Create(userId, NotificationTemplateKeys.QuizResult, title, body);
            notificationRepository.Add(notification);

            var pushToken = await deviceTokenClient.GetPushTokenAsync(userId.Value, ct);
            if (pushToken is not null)
            {
                notificationSender.Send(new NotificationPayload(
                    NotificationTemplateKeys.QuizResult,
                    pushToken,
                    new Dictionary<string, string>
                    {
                        ["isCorrect"] = isCorrect.ToString(),
                        ["question"] = quiz.Question,
                    }));
            }
        }

        await attemptRepository.SaveChangesAsync(ct);

        return QuizAttemptResponse.FromEntity(attempt);
    }
}
