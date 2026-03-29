using FluentAssertions;
using Hammer.Auction.Application.Exceptions;
using Hammer.Auction.Application.UseCases.DeleteQuiz;
using Hammer.Auction.Domain.Entities;
using Hammer.Auction.Domain.Ports;
using Hammer.Auction.Domain.ValueObjects;
using NSubstitute;

namespace Hammer.Auction.Tests.Application;

/// <summary>
/// Tests for <see cref="DeleteQuizUseCase"/>.
/// </summary>
public sealed class DeleteQuizUseCaseTests
{
    private readonly IQuizRepository _repository = Substitute.For<IQuizRepository>();
    private readonly DeleteQuizUseCase _sut;

    public DeleteQuizUseCaseTests()
    {
        _sut = new DeleteQuizUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingQuiz_ShouldRemoveAndPersistAsync()
    {
        Quiz quiz = CreateQuiz(1);
        _repository.GetByIdAsync(new QuizId(1), Arg.Any<CancellationToken>()).Returns(quiz);

        await _sut.ExecuteAsync(new QuizId(1));

        _repository.Received(1).Remove(quiz);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentQuiz_ShouldThrowNotFoundExceptionAsync()
    {
        _repository.GetByIdAsync(new QuizId(999), Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Func<Task> act = () => _sut.ExecuteAsync(new QuizId(999));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Quiz CreateQuiz(long id)
    {
        var quiz = Quiz.Create("Q", "A", "B", "C", "D", 0, "E");

        typeof(Quiz).GetProperty(nameof(Quiz.Id))!.SetValue(quiz, id);

        return quiz;
    }
}
