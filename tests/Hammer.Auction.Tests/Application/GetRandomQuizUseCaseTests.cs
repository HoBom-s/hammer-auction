using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.GetRandomQuiz;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetRandomQuizUseCase"/>.
/// </summary>
public sealed class GetRandomQuizUseCaseTests
{
    private readonly IQuizRepository _repository = Substitute.For<IQuizRepository>();
    private readonly GetRandomQuizUseCase _sut;

    public GetRandomQuizUseCaseTests()
    {
        _sut = new GetRandomQuizUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCount_ShouldReturnQuizzesAsync()
    {
        List<Quiz> quizzes = [CreateQuiz(1), CreateQuiz(2), CreateQuiz(3)];
        _repository.GetRandomAsync(3, Arg.Any<CancellationToken>()).Returns(quizzes);

        IReadOnlyList<QuizResponse> result = await _sut.ExecuteAsync(3);

        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task ExecuteAsync_WithInvalidCount_ShouldThrowBadRequestExceptionAsync(int count)
    {
        Func<Task> act = () => _sut.ExecuteAsync(count);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRepository_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetRandomAsync(3, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Quiz>());

        Func<Task> act = () => _sut.ExecuteAsync(3);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapResponseCorrectlyAsync()
    {
        Quiz quiz = CreateQuiz(1);
        _repository.GetRandomAsync(1, Arg.Any<CancellationToken>()).Returns(new[] { quiz });

        IReadOnlyList<QuizResponse> result = await _sut.ExecuteAsync(1);

        QuizResponse response = result[0];
        response.Question.Should().Be("Q1");
        response.Choices.Should().HaveCount(4);
        response.CorrectIndex.Should().Be(0);
        response.Explanation.Should().Be("E1");
    }

    private static Quiz CreateQuiz(long id)
    {
        var quiz = Quiz.Create($"Q{id}", "A", "B", "C", "D", 0, $"E{id}");

        typeof(Quiz).GetProperty(nameof(Quiz.Id))!.SetValue(quiz, id);

        return quiz;
    }
}
