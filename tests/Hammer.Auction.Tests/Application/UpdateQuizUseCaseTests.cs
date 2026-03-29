using FluentAssertions;
using Hammer.Auction.Application.Common;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.UpdateQuiz;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="UpdateQuizUseCase"/>.
/// </summary>
public sealed class UpdateQuizUseCaseTests
{
    private readonly IQuizRepository _repository = Substitute.For<IQuizRepository>();
    private readonly UpdateQuizUseCase _sut;

    public UpdateQuizUseCaseTests()
    {
        _sut = new UpdateQuizUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingQuiz_ShouldUpdateAndReturnAsync()
    {
        Quiz quiz = CreateQuiz(1);
        _repository.GetByIdAsync(new QuizId(1), Arg.Any<CancellationToken>()).Returns(quiz);
        UpdateQuizRequest request = new("Updated?", "X", "Y", "Z", "W", 3, "New explanation");

        QuizResponse result = await _sut.ExecuteAsync(new QuizId(1), request);

        result.Question.Should().Be("Updated?");
        result.Choices.Should().Equal("X", "Y", "Z", "W");
        result.CorrectIndex.Should().Be(3);
        result.Explanation.Should().Be("New explanation");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentQuiz_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetByIdAsync(new QuizId(999), Arg.Any<CancellationToken>()).Returns((Quiz?)null);
        UpdateQuizRequest request = new("Q", "A", "B", "C", "D", 0, "E");

        Func<Task> act = () => _sut.ExecuteAsync(new QuizId(999), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullExceptionAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(new QuizId(1), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static Quiz CreateQuiz(long id)
    {
        var quiz = Quiz.Create("Original?", "A", "B", "C", "D", 0, "Original");

        typeof(Quiz).GetProperty(nameof(Quiz.Id))!.SetValue(quiz, id);

        return quiz;
    }
}
