using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempt;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
///     Tests for <see cref="SubmitQuizAttemptUseCase" />.
/// </summary>
public sealed class SubmitQuizAttemptUseCaseTests
{
    private readonly IQuizAttemptRepository _attemptRepository = Substitute.For<IQuizAttemptRepository>();
    private readonly IQuizRepository _quizRepository = Substitute.For<IQuizRepository>();
    private readonly SubmitQuizAttemptUseCase _sut;

    public SubmitQuizAttemptUseCaseTests()
    {
        _sut = new SubmitQuizAttemptUseCase(_quizRepository, _attemptRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithCorrectAnswer_ShouldReturnIsCorrectTrueAsync()
    {
        Quiz quiz = CreateQuiz(1, 2);
        _quizRepository.GetByIdAsync(new QuizId(1), Arg.Any<CancellationToken>()).Returns(quiz);
        SubmitQuizAttemptRequest request = new(2);

        QuizAttemptResponse result = await _sut.ExecuteAsync(
            new UserId("user-1"),
            new QuizId(1),
            request);

        result.IsCorrect.Should().BeTrue();
        result.SelectedIndex.Should().Be(2);
        _attemptRepository.Received(1).Add(Arg.Any<QuizAttempt>());
        await _attemptRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongAnswer_ShouldReturnIsCorrectFalseAsync()
    {
        Quiz quiz = CreateQuiz(1, 2);
        _quizRepository.GetByIdAsync(new QuizId(1), Arg.Any<CancellationToken>()).Returns(quiz);
        SubmitQuizAttemptRequest request = new(0);

        QuizAttemptResponse result = await _sut.ExecuteAsync(
            new UserId("user-1"),
            new QuizId(1),
            request);

        result.IsCorrect.Should().BeFalse();
        result.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentQuiz_ShouldThrowNotFoundExceptionAsync()
    {
        _quizRepository.GetByIdAsync(new QuizId(999), Arg.Any<CancellationToken>()).Returns((Quiz?)null);
        SubmitQuizAttemptRequest request = new(0);

        Func<Task> act = () => _sut.ExecuteAsync(
            new UserId("user-1"),
            new QuizId(999),
            request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullExceptionAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(
            new UserId("user-1"),
            new QuizId(1),
            null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithAnonymousUser_ShouldSucceedAsync()
    {
        Quiz quiz = CreateQuiz(1, 0);
        _quizRepository.GetByIdAsync(new QuizId(1), Arg.Any<CancellationToken>()).Returns(quiz);
        SubmitQuizAttemptRequest request = new(0);

        QuizAttemptResponse result = await _sut.ExecuteAsync(
            UserId.Anonymous,
            new QuizId(1),
            request);

        result.IsCorrect.Should().BeTrue();
    }

    private static Quiz CreateQuiz(long id, int correctIndex = 0)
    {
        var quiz = Quiz.Create("Q", "A", "B", "C", "D", correctIndex, "E");

        typeof(Quiz).GetProperty(nameof(Quiz.Id))!.SetValue(quiz, id);

        return quiz;
    }
}
