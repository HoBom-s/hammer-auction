using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.UseCases.GetQuizzes;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="GetQuizzesUseCase"/>.
/// </summary>
public sealed class GetQuizzesUseCaseTests
{
    private readonly IQuizRepository _repository = Substitute.For<IQuizRepository>();
    private readonly GetQuizzesUseCase _sut;

    public GetQuizzesUseCaseTests()
    {
        _sut = new GetQuizzesUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPagedResponseAsync()
    {
        List<Quiz> quizzes = [CreateQuiz(1), CreateQuiz(2)];
        _repository.GetPagedAsync(1, 20, Arg.Any<CancellationToken>()).Returns((quizzes, 5));

        PagedResponse<QuizResponse> result = await _sut.ExecuteAsync(1, 20);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.Size.Should().Be(20);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyPageAsync()
    {
        _repository.GetPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Quiz>(), 0));

        PagedResponse<QuizResponse> result = await _sut.ExecuteAsync(1, 20);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapResponseFieldsAsync()
    {
        Quiz quiz = CreateQuiz(1);
        _repository.GetPagedAsync(1, 10, Arg.Any<CancellationToken>()).Returns((new[] { quiz }, 1));

        PagedResponse<QuizResponse> result = await _sut.ExecuteAsync(1, 10);

        QuizResponse item = result.Items[0];
        item.Question.Should().Be("Q1");
        item.Choices.Should().Equal("A", "B", "C", "D");
        item.CorrectIndex.Should().Be(0);
    }

    private static Quiz CreateQuiz(long id)
    {
        var quiz = Quiz.Create($"Q{id}", "A", "B", "C", "D", 0, $"E{id}");

        typeof(Quiz).GetProperty(nameof(Quiz.Id))!.SetValue(quiz, id);

        return quiz;
    }
}
