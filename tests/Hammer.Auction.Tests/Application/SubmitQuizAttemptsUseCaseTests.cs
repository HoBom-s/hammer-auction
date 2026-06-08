using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempts;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
///     Tests for <see cref="SubmitQuizAttemptsUseCase" />.
/// </summary>
public sealed class SubmitQuizAttemptsUseCaseTests
{
    private readonly IQuizAttemptRepository _attemptRepository = Substitute.For<IQuizAttemptRepository>();
    private readonly IDeviceTokenClient _deviceTokenClient = Substitute.For<IDeviceTokenClient>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly INotificationSender _notificationSender = Substitute.For<INotificationSender>();
    private readonly INotificationSettingRepository _notificationSettingRepository = Substitute.For<INotificationSettingRepository>();
    private readonly IQuizClient _quizClient = Substitute.For<IQuizClient>();
    private readonly SubmitQuizAttemptsUseCase _sut;

    public SubmitQuizAttemptsUseCaseTests()
    {
        _sut = new SubmitQuizAttemptsUseCase(
            _quizClient,
            _attemptRepository,
            _deviceTokenClient,
            _notificationSender,
            _notificationRepository,
            _notificationSettingRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedAnswers_ShouldTallyCorrectCountAsync()
    {
        _quizClient.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(1, 2));
        _quizClient.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(2, 0));
        _quizClient.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(3, 1));

        SubmitQuizAttemptsRequest request = new(
        [
            new QuizAttemptSubmission(1, 2), // correct
            new QuizAttemptSubmission(2, 3), // wrong
            new QuizAttemptSubmission(3, 1), // correct
        ]);

        SubmitQuizAttemptsResponse result = await _sut.ExecuteAsync(new UserId("user-1"), request);

        result.Total.Should().Be(3);
        result.Correct.Should().Be(2);
        result.Attempts.Should().HaveCount(3);
        _attemptRepository.Received(3).Add(Arg.Any<QuizAttempt>());
        await _attemptRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendSingleCompletionNotification_WhenDeviceTokenExistsAsync()
    {
        _quizClient.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(1, 2));
        _quizClient.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(2, 0));
        _deviceTokenClient.GetPushTokenAsync("user-1", Arg.Any<CancellationToken>())
            .Returns("ExponentPushToken[abc123]");

        SubmitQuizAttemptsRequest request = new(
        [
            new QuizAttemptSubmission(1, 2), // correct
            new QuizAttemptSubmission(2, 3), // wrong
        ]);

        await _sut.ExecuteAsync(new UserId("user-1"), request);

        _notificationSender.Received(1).Send(Arg.Is<NotificationPayload>(p =>
            p.TemplateKey == "quiz_completed"
            && p.RecipientToken == "ExponentPushToken[abc123]"
            && p.Variables["total"] == "2"
            && p.Variables["correct"] == "1"));
        _notificationRepository.Received(1).Add(Arg.Any<Notification>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipPush_ButPersistNotification_WhenNoDeviceTokenAsync()
    {
        _quizClient.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(1, 0));
        _deviceTokenClient.GetPushTokenAsync("user-1", Arg.Any<CancellationToken>())
            .Returns((string?)null);

        SubmitQuizAttemptsRequest request = new([new QuizAttemptSubmission(1, 0)]);

        await _sut.ExecuteAsync(new UserId("user-1"), request);

        _notificationRepository.Received(1).Add(Arg.Any<Notification>());
        _notificationSender.DidNotReceive().Send(Arg.Any<NotificationPayload>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotNotify_WhenNotificationsDisabledAsync()
    {
        _quizClient.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(1, 0));
        _notificationSettingRepository.GetByUserIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(NotificationSetting.Create(new UserId("user-1"), isEnabled: false));

        SubmitQuizAttemptsRequest request = new([new QuizAttemptSubmission(1, 0)]);

        await _sut.ExecuteAsync(new UserId("user-1"), request);

        _notificationRepository.DidNotReceive().Add(Arg.Any<Notification>());
        _notificationSender.DidNotReceive().Send(Arg.Any<NotificationPayload>());
        await _deviceTokenClient.DidNotReceive().GetPushTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentQuizInBatch_ShouldThrowNotFoundExceptionAsync()
    {
        _quizClient.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(CreateQuizResponse(1, 0));
        _quizClient.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((QuizResponse?)null);

        SubmitQuizAttemptsRequest request = new(
        [
            new QuizAttemptSubmission(1, 0),
            new QuizAttemptSubmission(999, 0),
        ]);

        Func<Task> act = () => _sut.ExecuteAsync(new UserId("user-1"), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyAttempts_ShouldThrowBadRequestExceptionAsync()
    {
        SubmitQuizAttemptsRequest request = new([]);

        Func<Task> act = () => _sut.ExecuteAsync(new UserId("user-1"), request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullExceptionAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(new UserId("user-1"), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static QuizResponse CreateQuizResponse(long id, int correctIndex = 0) =>
        new(id, "Q", ["A", "B", "C", "D"], correctIndex, "E");
}
