using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Application.UseCases.GetRandomQuiz;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetRandomQuizUseCase"/>.
/// </summary>
public sealed class GetRandomQuizUseCaseTests
{
    private readonly IQuizClient _quizClient = Substitute.For<IQuizClient>();
    private readonly GetRandomQuizUseCase _sut;

    public GetRandomQuizUseCaseTests()
    {
        _sut = new GetRandomQuizUseCase(_quizClient);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCount_ShouldReturnQuizzesAsync()
    {
        List<QuizResponse> quizzes = [CreateQuizResponse(1), CreateQuizResponse(2), CreateQuizResponse(3)];
        _quizClient.GetRandomAsync(3, Arg.Any<CancellationToken>()).Returns(quizzes);

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
    public async Task ExecuteAsync_WithEmptyResult_ShouldThrowNotFoundExceptionAsync()
    {
        _quizClient.GetRandomAsync(3, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<QuizResponse>());

        Func<Task> act = () => _sut.ExecuteAsync(3);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResponseCorrectlyAsync()
    {
        QuizResponse quiz = CreateQuizResponse(1);
        _quizClient.GetRandomAsync(1, Arg.Any<CancellationToken>()).Returns(new[] { quiz });

        IReadOnlyList<QuizResponse> result = await _sut.ExecuteAsync(1);

        QuizResponse response = result[0];
        response.Question.Should().Be("Q1");
        response.Choices.Should().HaveCount(4);
        response.CorrectIndex.Should().Be(0);
        response.Explanation.Should().Be("E1");
    }

    private static QuizResponse CreateQuizResponse(long id) =>
        new(id, $"Q{id}", ["A", "B", "C", "D"], 0, $"E{id}");
}
